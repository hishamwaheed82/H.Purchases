using H.Platform;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace H.Purchases;

[DependsOn(
    typeof(PurchasesDomainSharedModule),
    typeof(PlatformDomainModule),
    typeof(AbpDddDomainModule)
)]
public class PurchasesDomainModule : AbpModule
{
}
