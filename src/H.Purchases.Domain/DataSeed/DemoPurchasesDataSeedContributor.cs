using System;
using System.Threading.Tasks;
using H.Purchases.Suppliers;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace H.Purchases.DataSeed;

/// <summary>
/// Idempotent demo seed for a fresh tenant: two suppliers with contact fields.
/// Skips host; no-ops when any supplier already exists (never wipes production).
/// </summary>
public class DemoPurchasesDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Supplier, Guid> _suppliers;
    private readonly IGuidGenerator _guids;
    private readonly ICurrentTenant _currentTenant;

    public DemoPurchasesDataSeedContributor(
        IRepository<Supplier, Guid> suppliers,
        IGuidGenerator guids,
        ICurrentTenant currentTenant)
    {
        _suppliers     = suppliers;
        _guids         = guids;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId == null) return; // host: nothing
        using (_currentTenant.Change(context.TenantId))
        {
            if (await _suppliers.AnyAsync()) return;

            var samples = new[]
            {
                ("SUP001", "المورد الافتراضي", "01100000001", "supplier1@demo.local"),
                ("SUP002", "شركة التوريد الحديث", "01100000002", "modern@demo.local")
            };

            foreach (var (code, name, phone, email) in samples)
            {
                var supplier = new Supplier(_guids.Create())
                {
                    TenantId = context.TenantId,
                    Code     = code,
                    Name     = name,
                    Phone    = phone,
                    Email    = email,
                    IsActive = true
                };
                supplier.EnsureValid();
                await _suppliers.InsertAsync(supplier);
            }
        }
    }
}
