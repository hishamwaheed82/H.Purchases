namespace H.Purchases;

public static class PurchasesDbProperties
{
    public static string DbTablePrefix { get; set; } = "";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Purchases";
}
