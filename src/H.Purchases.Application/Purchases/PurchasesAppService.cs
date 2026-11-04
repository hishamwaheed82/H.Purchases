using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H.Purchases.Invoices;
using H.Purchases.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace H.Purchases.Purchases;

[Authorize]
public class PurchasesAppService : H.Purchases.PurchasesAppService, IPurchasesAppService
{
    private readonly IRepository<Supplier, Guid>        _suppliers;
    private readonly IRepository<PurchaseInvoice, Guid> _invoices;
    private readonly PurchaseInvoiceManager             _manager;
    private readonly IGuidGenerator                     _guids;
    private readonly PurchasesGlPoster                  _glPoster;
    private readonly PurchasesStockPoster               _stockPoster;
    private readonly InventoryStockClient               _inventoryClient;

    public PurchasesAppService(
        IRepository<Supplier, Guid>        suppliers,
        IRepository<PurchaseInvoice, Guid> invoices,
        PurchaseInvoiceManager             manager,
        IGuidGenerator                     guids,
        PurchasesGlPoster                  glPoster,
        PurchasesStockPoster               stockPoster,
        InventoryStockClient               inventoryClient)
    {
        _suppliers       = suppliers;
        _invoices        = invoices;
        _manager         = manager;
        _guids           = guids;
        _glPoster        = glPoster;
        _stockPoster     = stockPoster;
        _inventoryClient = inventoryClient;
    }

    // ── Bootstrap ─────────────────────────────────────────────────────────────

    public virtual async Task<PurchasesBootstrapDto> GetBootstrapAsync()
    {
        var suppliers = await _suppliers.GetListAsync();
        var query     = await _invoices.WithDetailsAsync(i => i.Lines);
        var recent    = (await AsyncExecuter.ToListAsync(query))
            .OrderByDescending(i => i.DocDate).ThenByDescending(i => i.Number)
            .Take(40).ToList();

        // Best-effort Inventory lookups (H_Ops_Machine). Empty when Inventory is down.
        var warehouses = await _inventoryClient.ListWarehousesAsync(CurrentTenant.Id);
        var items      = await _inventoryClient.ListItemsAsync(CurrentTenant.Id);

        return new PurchasesBootstrapDto
        {
            Suppliers      = suppliers.Select(MapSupplier).ToList(),
            RecentInvoices = recent.Select(i => MapInvoice(i, suppliers)).ToList(),
            Warehouses     = warehouses,
            Items          = items
        };
    }

    // ── Supplier CRUD ─────────────────────────────────────────────────────────

    public virtual async Task<SupplierDto> CreateSupplierAsync(CreateUpdateSupplierDto input)
    {
        if (await _suppliers.AnyAsync(s => s.Code == input.Code))
            throw new BusinessException(PurchasesErrorCodes.DuplicateSupplierCode).WithData("Code", input.Code);

        var supplier = new Supplier(_guids.Create())
        {
            TenantId = CurrentTenant.Id,
            Code     = input.Code.Trim(),
            Name     = input.Name.Trim(),
            Phone    = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim(),
            Email    = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim(),
            IsActive = input.IsActive
        };
        supplier.EnsureValid();
        await _suppliers.InsertAsync(supplier, autoSave: true);
        return MapSupplier(supplier);
    }

    public virtual async Task<SupplierDto> UpdateSupplierAsync(Guid id, CreateUpdateSupplierDto input)
    {
        var supplier = await _suppliers.GetAsync(id);
        if (await _suppliers.AnyAsync(s => s.Code == input.Code && s.Id != id))
            throw new BusinessException(PurchasesErrorCodes.DuplicateSupplierCode).WithData("Code", input.Code);

        supplier.Code     = input.Code.Trim();
        supplier.Name     = input.Name.Trim();
        supplier.Phone    = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim();
        supplier.Email    = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
        supplier.IsActive = input.IsActive;
        supplier.EnsureValid();
        await _suppliers.UpdateAsync(supplier, autoSave: true);
        return MapSupplier(supplier);
    }

    // ── Invoice CRUD ──────────────────────────────────────────────────────────

    public virtual async Task<PurchaseInvoiceDto> CreateInvoiceAsync(CreatePurchaseInvoiceDto input)
    {
        if (!await _suppliers.AnyAsync(s => s.Id == input.SupplierId))
            throw new BusinessException(PurchasesErrorCodes.SupplierNotFound);

        if (!input.Lines.Any())
            throw new BusinessException(PurchasesErrorCodes.NoLines);

        var suppliers = await _suppliers.GetListAsync();

        var invoice = new PurchaseInvoice(_guids.Create())
        {
            TenantId   = CurrentTenant.Id,
            Number     = await _manager.NextNumberAsync(),
            DocDate    = input.DocDate.Date,
            SupplierId = input.SupplierId,
            WarehouseId= input.WarehouseId,
            TaxTotal   = input.TaxTotal,
            Status     = PurchaseInvoiceStatus.Draft
        };

        var n = 1;
        foreach (var l in input.Lines)
        {
            var line = new PurchaseInvoiceLine(_guids.Create())
            {
                InvoiceId   = invoice.Id,
                LineNumber  = n++,
                ItemId      = l.ItemId,
                Description = l.Description,
                Qty         = l.Qty,
                UnitCost    = l.UnitCost
            };
            line.EnsureValid();
            invoice.Lines.Add(line);
        }

        invoice.RecalcTotals();
        await _invoices.InsertAsync(invoice, autoSave: true);

        if (input.Post)
        {
            await _manager.PostAsync(invoice, CurrentUser.Id);
            // Stock receipt — best-effort; non-blocking unless RequireStock=true.
            await _stockPoster.PostReceiptAsync(invoice, CurrentTenant.Id);
            // GL post — fire-and-forget on failure (non-blocking; logs warning on error).
            await _glPoster.PostInvoiceJournalAsync(invoice, CurrentTenant.Id);
        }

        return MapInvoice(invoice, suppliers);
    }

    public virtual async Task<PurchaseInvoiceDto> PostInvoiceAsync(Guid id)
    {
        var query   = await _invoices.WithDetailsAsync(i => i.Lines);
        var where   = query.Where(i => i.Id == id);
        var invoice = await AsyncExecuter.FirstOrDefaultAsync(where)
            ?? throw new BusinessException(PurchasesErrorCodes.InvoiceNotFound);

        await _manager.PostAsync(invoice, CurrentUser.Id);

        // Stock receipt — best-effort; non-blocking unless RequireStock=true.
        await _stockPoster.PostReceiptAsync(invoice, CurrentTenant.Id);
        // GL post — fire-and-forget on failure (non-blocking; logs warning on error).
        await _glPoster.PostInvoiceJournalAsync(invoice, CurrentTenant.Id);

        var suppliers = await _suppliers.GetListAsync();
        return MapInvoice(invoice, suppliers);
    }

    public virtual async Task<PurchaseInvoiceDto> CancelInvoiceAsync(Guid id)
    {
        var query   = await _invoices.WithDetailsAsync(i => i.Lines);
        var where   = query.Where(i => i.Id == id);
        var invoice = await AsyncExecuter.FirstOrDefaultAsync(where)
            ?? throw new BusinessException(PurchasesErrorCodes.InvoiceNotFound);

        await _manager.CancelAsync(invoice);

        var suppliers = await _suppliers.GetListAsync();
        return MapInvoice(invoice, suppliers);
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static SupplierDto MapSupplier(Supplier s) => new()
    {
        Id       = s.Id,
        Code     = s.Code,
        Name     = s.Name,
        Phone    = s.Phone,
        Email    = s.Email,
        IsActive = s.IsActive
    };

    private static PurchaseInvoiceDto MapInvoice(PurchaseInvoice i, IEnumerable<Supplier> suppliers)
    {
        var map = suppliers.ToDictionary(s => s.Id);
        var sup = map.GetValueOrDefault(i.SupplierId);
        return new PurchaseInvoiceDto
        {
            Id              = i.Id,
            Number          = i.Number,
            DocDate         = i.DocDate,
            SupplierId      = i.SupplierId,
            SupplierCode    = sup?.Code,
            SupplierName    = sup?.Name,
            WarehouseId     = i.WarehouseId,
            Status          = i.Status,
            SubTotal        = i.SubTotal,
            TaxTotal        = i.TaxTotal,
            GrandTotal      = i.GrandTotal,
            JournalNumber   = i.JournalNumber,
            StockDocumentId = i.StockDocumentId,
            Lines = i.Lines
                .OrderBy(l => l.LineNumber)
                .Select(l => new PurchaseInvoiceLineDto
                {
                    Id          = l.Id,
                    LineNumber  = l.LineNumber,
                    ItemId      = l.ItemId,
                    Description = l.Description,
                    Qty         = l.Qty,
                    UnitCost    = l.UnitCost,
                    LineTotal   = l.LineTotal
                }).ToList()
        };
    }
}
