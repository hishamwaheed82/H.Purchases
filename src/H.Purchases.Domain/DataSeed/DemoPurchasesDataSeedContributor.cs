using System;
using System.Threading.Tasks;
using H.Purchases.Suppliers;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace H.Purchases.DataSeed;

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

            var supplier = new Supplier(_guids.Create())
            {
                TenantId = context.TenantId,
                Code     = "SUP001",
                Name     = "المورد الافتراضي",
                IsActive = true
            };
            supplier.EnsureValid();
            await _suppliers.InsertAsync(supplier);
        }
    }
}
