using H.Purchases.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;

namespace H.Purchases.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(PurchasesEntityFrameworkCoreModule),
    typeof(PurchasesDomainModule)
)]
public class PurchasesDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbContextOptions>(options => options.UseSqlServer());
    }
}
