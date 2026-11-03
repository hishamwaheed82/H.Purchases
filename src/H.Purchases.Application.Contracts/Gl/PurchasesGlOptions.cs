namespace H.Purchases.Gl;

/// <summary>
/// Configuration for Purchases → Accounting GL posting.
/// Section name: "PurchasesGl"
///
/// Demo / default account codes (Chart of Accounts) can be overridden per deployment.
/// All codes must exist in H.Accounting for the tenant before invoice-post fires.
/// </summary>
public class PurchasesGlOptions
{
    public const string SectionName = "PurchasesGl";

    // ── Accounting API ────────────────────────────────────────────────────────

    /// <summary>Base URL of the H.Accounting HttpApi.Host, e.g. https://localhost:44400</summary>
    public string AccountingBaseUrl { get; set; } = string.Empty;

    // ── Machine client (client_credentials) ──────────────────────────────────

    /// <summary>
    /// OAuth client_id for the Ops machine client.
    /// Default: H_Ops_Machine (shared cross-module machine client seeded in H.Auth).
    /// </summary>
    public string ClientId { get; set; } = "H_Ops_Machine";

    /// <summary>
    /// OAuth client_secret.
    /// Store in appsettings.secrets.json or environment variable; never commit.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Scope requested in the client_credentials grant. Accounting API audience.</summary>
    public string Scope { get; set; } = "H.Accounting";

    // ── Journal type ─────────────────────────────────────────────────────────

    /// <summary>Journal type code. Must exist in Accounting journal-types.</summary>
    public string JournalType { get; set; } = "GJ";

    // ── Default account codes (demo Chart of Accounts) ────────────────────────

    /// <summary>
    /// Inventory / purchases asset account debited on invoice post.
    /// Used when lines contain stock items (ItemId is set).
    /// Demo default: 1401 (Inventory / Merchandise Stock).
    /// </summary>
    public string InventoryAccount { get; set; } = "1401";

    /// <summary>
    /// Purchases expense account debited on invoice post for free-text / service lines
    /// (ItemId is null). Falls back to InventoryAccount when empty.
    /// Demo default: 5201 (Purchases Expense / Cost of Purchases).
    /// </summary>
    public string PurchasesExpenseAccount { get; set; } = "5201";

    /// <summary>
    /// Accounts Payable account credited on invoice post (supplier owes).
    /// Demo default: 2102 (Accounts Payable — Suppliers).
    /// </summary>
    public string AccountsPayableAccount { get; set; } = "2102";
}
