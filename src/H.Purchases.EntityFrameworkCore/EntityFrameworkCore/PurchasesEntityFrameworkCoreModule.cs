using H.Platform.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace H.Purchases.EntityFrameworkCore;

[DependsOn(
    typeof(PurchasesDomainModule),
    typeof(PlatformEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class PurchasesEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<PurchasesDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}
