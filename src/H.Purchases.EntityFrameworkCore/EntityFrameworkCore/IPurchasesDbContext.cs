using H.Purchases.Invoices;
using H.Purchases.Suppliers;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace H.Purchases.EntityFrameworkCore;

public interface IPurchasesDbContext : IEfCoreDbContext
{
    DbSet<Supplier>            Suppliers    { get; }
    DbSet<PurchaseInvoice>     Invoices     { get; }
    DbSet<PurchaseInvoiceLine> InvoiceLines { get; }
}
