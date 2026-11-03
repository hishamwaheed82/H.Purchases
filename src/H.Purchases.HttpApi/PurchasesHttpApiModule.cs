using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace H.Purchases;

[DependsOn(
    typeof(PurchasesApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class PurchasesHttpApiModule : AbpModule
{
}
