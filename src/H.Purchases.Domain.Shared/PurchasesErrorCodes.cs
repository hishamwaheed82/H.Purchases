namespace H.Purchases;

public static class PurchasesErrorCodes
{
    public const string DuplicateSupplierCode   = "Purchases:40001";
    public const string InvoiceNotDraft         = "Purchases:40002";
    public const string InvoiceAlreadyPosted    = "Purchases:40003";
    public const string SupplierNotFound        = "Purchases:40004";
    public const string InvoiceNotFound         = "Purchases:40005";
    public const string NoLines                 = "Purchases:40006";
    public const string InvalidQuantity         = "Purchases:40007";
    public const string InvalidUnitCost         = "Purchases:40008";
}
