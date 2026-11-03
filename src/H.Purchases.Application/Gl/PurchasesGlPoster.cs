using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H.Purchases.Gl;
using H.Purchases.Invoices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Domain.Repositories;

namespace H.Purchases;

/// <summary>
/// Builds a purchase invoice journal from a posted PurchaseInvoice and sends it to H.Accounting GL.
///
/// Journal shape:
///
///   INVOICE POST:
///     DEBIT  InventoryAccount / PurchasesExpenseAccount   grandTotal   — asset/expense
///     CREDIT AccountsPayableAccount                        grandTotal   — supplier liability
///
/// Stock lines (ItemId set) → InventoryAccount
/// Service/free-text lines (ItemId null) → PurchasesExpenseAccount (falls back to InventoryAccount if empty)
///
/// For simplicity the full GrandTotal (including tax) is posted as a single debit leg;
/// tax split can be extended later using a TaxPayableAccount option.
///
/// SourceKey = "pi:{id}" — idempotent via Accounting SourceKey uniqueness contract.
/// </summary>
public class PurchasesGlPoster
{
    private readonly AccountingGlClient _glClient;
    private readonly IOptions<PurchasesGlOptions> _options;
    private readonly IRepository<PurchaseInvoice, Guid> _invoices;
    private readonly ILogger<PurchasesGlPoster> _logger;

    public PurchasesGlPoster(
        AccountingGlClient glClient,
        IOptions<PurchasesGlOptions> options,
        IRepository<PurchaseInvoice, Guid> invoices,
        ILogger<PurchasesGlPoster> logger)
    {
        _glClient = glClient;
        _options  = options;
        _invoices = invoices;
        _logger   = logger;
    }

    /// <summary>
    /// Posts (or skips) the purchase invoice GL journal.
    /// Never throws — logs and returns null on failure.
    /// </summary>
    /// <param name="invoice">The posted PurchaseInvoice entity (must already be saved).</param>
    /// <param name="tenantId">Tenant ID forwarded to the Accounting API header.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>GL journal number if posted successfully, null otherwise.</returns>
    public async Task<string?> PostInvoiceJournalAsync(
        PurchaseInvoice invoice,
        Guid? tenantId,
        CancellationToken ct = default)
    {
        var opts = _options.Value;

        var grandTotal = invoice.GrandTotal;
        if (grandTotal <= 0m)
        {
            _logger.LogInformation("[Purchases→GL] Invoice {Number} has zero grand total — skipping GL post.", invoice.Number);
            return null;
        }

        var sourceKey = BuildSourceKey(invoice.Id);

        // Idempotency guard on Purchases side
        if (!string.IsNullOrWhiteSpace(invoice.JournalNumber))
        {
            _logger.LogInformation("[Purchases→GL] Invoice {Number} already has GL journal {Journal} — skipping.",
                invoice.Number, invoice.JournalNumber);
            return invoice.JournalNumber;
        }

        var date  = invoice.DocDate.ToString("yyyy-MM-dd");
        var lines = BuildLines(opts, invoice);

        if (lines.Count == 0)
        {
            _logger.LogWarning("[Purchases→GL] No GL lines could be built for invoice {Number} — skipping.", invoice.Number);
            return null;
        }

        var request = new CreateGlJournalRequest
        {
            Date         = date,
            Ref          = invoice.Number,
            Type         = opts.JournalType,
            DescAr       = $"فاتورة مشتريات {invoice.Number}",
            DescEn       = $"Purchase invoice {invoice.Number}",
            Status       = "posted",
            SourceModule = "Purchases",
            SourceKey    = sourceKey,
            Lines        = lines
        };

        var (success, journalNumber, alreadyExisted) = await _glClient.CreateJournalAsync(request, tenantId, ct);

        if (success)
        {
            var effective = journalNumber ?? $"ACC-{sourceKey}";
            try
            {
                invoice.JournalNumber = effective;
                await _invoices.UpdateAsync(invoice, autoSave: true);
            }
            catch (Exception ex)
            {
                // Non-fatal: GL already posted; we just couldn't persist the reference locally.
                _logger.LogWarning(ex, "[Purchases→GL] GL posted ({Journal}) but could not save JournalNumber on invoice {Number}.",
                    effective, invoice.Number);
            }

            return alreadyExisted ? invoice.JournalNumber : journalNumber;
        }

        _logger.LogWarning("[Purchases→GL] GL post failed for invoice {Number}. Invoice is posted but no journal was created.", invoice.Number);
        return null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Stable source key for idempotency.</summary>
    public static string BuildSourceKey(Guid invoiceId) => $"pi:{invoiceId}";

    private static List<GlLineRequest> BuildLines(PurchasesGlOptions opts, PurchaseInvoice invoice)
    {
        var lines = new List<GlLineRequest>();

        if (string.IsNullOrWhiteSpace(opts.AccountsPayableAccount))
            return lines;

        // Determine whether this is a stock or service invoice by inspecting lines.
        // If any line has an ItemId → treat as inventory purchase.
        // If all lines are free-text → use PurchasesExpenseAccount.
        bool hasStockLines    = invoice.Lines.Any(l => l.ItemId.HasValue);
        var debitAcct         = hasStockLines
            ? opts.InventoryAccount
            : (string.IsNullOrWhiteSpace(opts.PurchasesExpenseAccount)
                ? opts.InventoryAccount
                : opts.PurchasesExpenseAccount);

        if (string.IsNullOrWhiteSpace(debitAcct))
            return lines;

        var grandTotal = invoice.GrandTotal;

        lines.Add(new GlLineRequest { Acct = debitAcct,                    Debit  = grandTotal, Desc = hasStockLines ? "مخزون مشتريات" : "مصروف مشتريات / خدمات" });
        lines.Add(new GlLineRequest { Acct = opts.AccountsPayableAccount,  Credit = grandTotal, Desc = "دائنون — موردون" });

        return lines;
    }
}
