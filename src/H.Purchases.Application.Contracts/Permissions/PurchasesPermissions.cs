namespace H.Purchases.Permissions;

public static class PurchasesPermissions
{
    public const string GroupName = "Purchases";

    public static class Suppliers
    {
        public const string Default = GroupName + ".Suppliers";
        public const string Create  = Default + ".Create";
        public const string Update  = Default + ".Update";
        public const string Delete  = Default + ".Delete";
    }

    public static class Invoices
    {
        public const string Default = GroupName + ".Invoices";
        public const string Create  = Default + ".Create";
        public const string Update  = Default + ".Update";
        public const string Post    = Default + ".Post";
    }
}
