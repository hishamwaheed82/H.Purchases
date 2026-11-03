using H.Purchases.Invoices;
using H.Purchases.Suppliers;
using H.Platform.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;

namespace H.Purchases.EntityFrameworkCore;

[ConnectionStringName(PurchasesDbProperties.ConnectionStringName)]
public class PurchasesDbContext : SoftPlanAbpDbContext<PurchasesDbContext>, IPurchasesDbContext
{
    public DbSet<Supplier>            Suppliers    { get; set; } = null!;
    public DbSet<PurchaseInvoice>     Invoices     { get; set; } = null!;
    public DbSet<PurchaseInvoiceLine> InvoiceLines { get; set; } = null!;

    public PurchasesDbContext(DbContextOptions<PurchasesDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigurePurchases();
    }
}
