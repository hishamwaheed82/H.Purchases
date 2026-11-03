using H.Platform;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace H.Purchases;

[DependsOn(
    typeof(PurchasesDomainSharedModule),
    typeof(PlatformApplicationContractsModule),
    typeof(AbpDddApplicationContractsModule)
)]
public class PurchasesApplicationContractsModule : AbpModule
{
}
