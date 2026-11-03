using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace H.Purchases.Invoices;

public class PurchaseInvoiceManager : DomainService
{
    private readonly IRepository<PurchaseInvoice, Guid> _invoices;

    public PurchaseInvoiceManager(IRepository<PurchaseInvoice, Guid> invoices)
    {
        _invoices = invoices;
    }

    public async Task<string> NextNumberAsync()
    {
        var count = await _invoices.CountAsync();
        return $"PI-{(count + 1):D6}";
    }

    /// <summary>
    /// Marks the invoice Posted.
    ///
    /// Integration stubs (wire up once the APIs are ready):
    ///
    ///   TODO: Inventory — StockReceipt
    ///         Call IInventoryIntegration.CreateStockReceiptAsync(
    ///             sourceModule: "Purchases", sourceKey: $"pi:{invoice.Id}",
    ///             warehouseId: invoice.WarehouseId, lines: ...)
    ///         Store the returned StockDocumentId on invoice.StockDocumentId.
    ///
    ///   TODO: Accounting — GL post
    ///         Call IAccountingIntegration.PostAsync(
    ///             sourceModule: "Purchases", sourceKey: $"pi:{invoice.Id}",
    ///             grandTotal: invoice.GrandTotal, supplierId: invoice.SupplierId, ...)
    ///         Store the returned JournalNumber on invoice.JournalNumber.
    /// </summary>
    public async Task PostAsync(PurchaseInvoice invoice, Guid? userId)
    {
        Check.NotNull(invoice, nameof(invoice));

        if (invoice.Status == PurchaseInvoiceStatus.Posted)
            throw new BusinessException(PurchasesErrorCodes.InvoiceAlreadyPosted);

        if (invoice.Status != PurchaseInvoiceStatus.Draft)
            throw new BusinessException(PurchasesErrorCodes.InvoiceNotDraft);

        if (!invoice.Lines.Any())
            throw new BusinessException(PurchasesErrorCodes.NoLines);

        foreach (var line in invoice.Lines)
            line.EnsureValid();

        invoice.RecalcTotals();
        invoice.Status = PurchaseInvoiceStatus.Posted;

        await _invoices.UpdateAsync(invoice);
    }
}
