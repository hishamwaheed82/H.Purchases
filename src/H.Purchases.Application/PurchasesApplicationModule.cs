using H.Platform;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;

namespace H.Purchases;

[DependsOn(
    typeof(PurchasesDomainModule),
    typeof(PurchasesApplicationContractsModule),
    typeof(PlatformApplicationModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpIdentityApplicationModule)
)]
public class PurchasesApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<PurchasesApplicationModule>(validate: false);
        });
    }
}
