using H.Purchases.Invoices;
using H.Purchases.Suppliers;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace H.Purchases.EntityFrameworkCore;

public static class PurchasesDbContextModelCreatingExtensions
{
    private const string MoneyType = "decimal(18,4)";
    private const string QtyType   = "decimal(18,4)";

    public static void ConfigurePurchases(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        var prefix = PurchasesDbProperties.DbTablePrefix;
        var schema  = PurchasesDbProperties.DbSchema;

        builder.Entity<Supplier>(b =>
        {
            b.ToTable(prefix + "Suppliers", schema);
            b.ConfigureByConvention();
            b.Property(x => x.AttachCount).HasDefaultValue(0);
            b.Property(x => x.NoteCount).HasDefaultValue(0);
            b.Property(x => x.CommentCount).HasDefaultValue(0);
            b.Property(x => x.Code).IsRequired().HasMaxLength(32);
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<PurchaseInvoice>(b =>
        {
            b.ToTable(prefix + "PurchaseInvoices", schema);
            b.ConfigureByConvention();
            b.Property(x => x.AttachCount).HasDefaultValue(0);
            b.Property(x => x.NoteCount).HasDefaultValue(0);
            b.Property(x => x.CommentCount).HasDefaultValue(0);
            b.Property(x => x.Number).IsRequired().HasMaxLength(32);
            b.Property(x => x.JournalNumber).HasMaxLength(64);
            b.Property(x => x.StockDocumentId).HasMaxLength(64);
            b.Property(x => x.SubTotal).HasColumnType(MoneyType);
            b.Property(x => x.TaxTotal).HasColumnType(MoneyType);
            b.Property(x => x.GrandTotal).HasColumnType(MoneyType);
            b.HasIndex(x => new { x.TenantId, x.Number }).IsUnique().HasFilter("[IsDeleted] = 0");
            b.HasIndex(x => new { x.TenantId, x.DocDate, x.Status });
            b.HasIndex(x => new { x.TenantId, x.SupplierId });
            b.HasMany(x => x.Lines)
             .WithOne(x => x.Invoice)
             .HasForeignKey(x => x.InvoiceId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PurchaseInvoiceLine>(b =>
        {
            b.ToTable(prefix + "PurchaseInvoiceLines", schema);
            b.ConfigureByConvention();
            b.Property(x => x.AttachCount).HasDefaultValue(0);
            b.Property(x => x.NoteCount).HasDefaultValue(0);
            b.Property(x => x.CommentCount).HasDefaultValue(0);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.Qty).HasColumnType(QtyType);
            b.Property(x => x.UnitCost).HasColumnType(MoneyType);
            b.Property(x => x.LineTotal).HasColumnType(MoneyType);
            b.HasIndex(x => new { x.InvoiceId, x.LineNumber });
        });
    }
}
