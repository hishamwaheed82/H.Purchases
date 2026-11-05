using System;
using System.Collections.Generic;
using System.Linq;
using H.Platform.Common;
using Volo.Abp;

namespace H.Purchases.Invoices;

/// <summary>
/// Purchase invoice header.
/// Workflow: Draft → Posted, or Draft → Cancelled.
/// When posted:
///   - PurchasesStockPoster: H.Inventory Receipt document (SourceKey=pi:{id}, best-effort)
///   - PurchasesGlPoster:    H.Accounting GL journal    (SourceKey=pi:{id}, best-effort)
/// </summary>
public class PurchaseInvoice : HTrackedAggregateRoot<Guid>
{
    public string Number { get; set; } = string.Empty;

    public DateTime DocDate { get; set; }

    public Guid SupplierId { get; set; }

    /// <summary>Destination warehouse for the stock receipt (optional in draft).</summary>
    public Guid? WarehouseId { get; set; }

    public PurchaseInvoiceStatus Status { get; set; } = PurchaseInvoiceStatus.Draft;

    public decimal SubTotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal GrandTotal { get; set; }

    /// <summary>GL journal number, populated after Accounting integration (PurchasesGlPoster).</summary>
    public string? JournalNumber { get; set; }

    /// <summary>Stock receipt document ID returned from Inventory integration (PurchasesStockPoster).</summary>
    public string? StockDocumentId { get; set; }

    public ICollection<PurchaseInvoiceLine> Lines { get; set; } = new List<PurchaseInvoiceLine>();

    protected PurchaseInvoice() { }

    public PurchaseInvoice(Guid id) : base(id) { }

    public void EnsureCanEdit()
    {
        if (Status != PurchaseInvoiceStatus.Draft)
            throw new BusinessException(PurchasesErrorCodes.InvoiceNotDraft);
    }

    public void RecalcTotals()
    {
        SubTotal   = Lines.Sum(l => l.LineTotal);
        GrandTotal = SubTotal + TaxTotal;
    }
}

/// <summary>
/// One line of a purchase invoice.
/// Either ItemId (stock item) or Description (free-text service) must be provided.
/// </summary>
public class PurchaseInvoiceLine : HTrackedAggregateRoot<Guid>
{
    public Guid InvoiceId { get; set; }

    public int LineNumber { get; set; }

    /// <summary>Inventory item (optional; null for free-text service lines).</summary>
    public Guid? ItemId { get; set; }

    public string? Description { get; set; }

    public decimal Qty { get; set; }

    public decimal UnitCost { get; set; }

    public decimal LineTotal { get; set; }

    public PurchaseInvoice? Invoice { get; set; }

    protected PurchaseInvoiceLine() { }

    public PurchaseInvoiceLine(Guid id) : base(id) { }

    public void EnsureValid()
    {
        if (Qty <= 0)
            throw new BusinessException(PurchasesErrorCodes.InvalidQuantity);
        if (UnitCost < 0)
            throw new BusinessException(PurchasesErrorCodes.InvalidUnitCost);
        LineTotal = Qty * UnitCost;
    }
}
