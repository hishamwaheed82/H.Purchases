using System;
using System.Threading.Tasks;
using H.Purchases.Purchases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace H.Purchases.HttpApi.V1;

/// <summary>Stable Vite-friendly routes under /api/v1/*.</summary>
[RemoteService]
[Area("purchases")]
[Route("api/v1")]
[Authorize]
public class PurchasesV1Controller : PurchasesController
{
    private readonly IPurchasesAppService _purchases;

    public PurchasesV1Controller(IPurchasesAppService purchases)
    {
        _purchases = purchases;
    }

    [HttpGet("bootstrap")]
    public Task<PurchasesBootstrapDto> BootstrapAsync() =>
        _purchases.GetBootstrapAsync();

    // ── Suppliers ─────────────────────────────────────────────────────────────

    [HttpGet("suppliers")]
    public async Task<object> SuppliersAsync()
    {
        var b = await _purchases.GetBootstrapAsync();
        return b.Suppliers;
    }

    [HttpPost("suppliers")]
    public Task<SupplierDto> CreateSupplierAsync([FromBody] CreateUpdateSupplierDto input) =>
        _purchases.CreateSupplierAsync(input);

    [HttpPut("suppliers/{id:guid}")]
    public Task<SupplierDto> UpdateSupplierAsync(Guid id, [FromBody] CreateUpdateSupplierDto input) =>
        _purchases.UpdateSupplierAsync(id, input);

    // ── Invoices ──────────────────────────────────────────────────────────────

    [HttpGet("invoices")]
    public async Task<object> InvoicesAsync()
    {
        var b = await _purchases.GetBootstrapAsync();
        return b.RecentInvoices;
    }

    [HttpPost("invoices")]
    public Task<PurchaseInvoiceDto> CreateInvoiceAsync([FromBody] CreatePurchaseInvoiceDto input) =>
        _purchases.CreateInvoiceAsync(input);

    [HttpPost("invoices/{id:guid}/post")]
    public Task<PurchaseInvoiceDto> PostInvoiceAsync(Guid id) =>
        _purchases.PostInvoiceAsync(id);
}
