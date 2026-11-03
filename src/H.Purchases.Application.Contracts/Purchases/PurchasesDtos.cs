using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace H.Purchases.Purchases;

// ── Supplier ──────────────────────────────────────────────────────────────────

public class SupplierDto
{
    public Guid   Id       { get; set; }
    public string Code     { get; set; } = "";
    public string Name     { get; set; } = "";
    public bool   IsActive { get; set; }
}

public class CreateUpdateSupplierDto
{
    [Required, MaxLength(32)]  public string Code     { get; set; } = "";
    [Required, MaxLength(256)] public string Name     { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

// ── Invoice Lines ─────────────────────────────────────────────────────────────

public class PurchaseInvoiceLineDto
{
    public Guid?   Id          { get; set; }
    public int     LineNumber  { get; set; }
    public Guid?   ItemId      { get; set; }
    public string? Description { get; set; }
    public decimal Qty         { get; set; }
    public decimal UnitCost    { get; set; }
    public decimal LineTotal   { get; set; }
}

public class CreatePurchaseInvoiceLineDto
{
    public Guid?   ItemId      { get; set; }
    public string? Description { get; set; }
    public decimal Qty         { get; set; }
    public decimal UnitCost    { get; set; }
}

// ── Invoice ───────────────────────────────────────────────────────────────────

public class PurchaseInvoiceDto
{
    public Guid                     Id              { get; set; }
    public string                   Number          { get; set; } = "";
    public DateTime                 DocDate         { get; set; }
    public Guid                     SupplierId      { get; set; }
    public string?                  SupplierCode    { get; set; }
    public string?                  SupplierName    { get; set; }
    public Guid?                    WarehouseId     { get; set; }
    public PurchaseInvoiceStatus    Status          { get; set; }
    public decimal                  SubTotal        { get; set; }
    public decimal                  TaxTotal        { get; set; }
    public decimal                  GrandTotal      { get; set; }
    public string?                  JournalNumber   { get; set; }
    public string?                  StockDocumentId { get; set; }
    public List<PurchaseInvoiceLineDto> Lines       { get; set; } = new();
}

public class CreatePurchaseInvoiceDto
{
    public DateTime DocDate      { get; set; } = DateTime.Today;
    [Required] public Guid SupplierId { get; set; }
    public Guid?    WarehouseId  { get; set; }
    public decimal  TaxTotal     { get; set; }
    /// <summary>When true the invoice is posted immediately after creation.</summary>
    public bool     Post         { get; set; }
    public List<CreatePurchaseInvoiceLineDto> Lines { get; set; } = new();
}

// ── Bootstrap ─────────────────────────────────────────────────────────────────

public class PurchasesBootstrapDto
{
    public List<SupplierDto>         Suppliers      { get; set; } = new();
    public List<PurchaseInvoiceDto>  RecentInvoices { get; set; } = new();
}
