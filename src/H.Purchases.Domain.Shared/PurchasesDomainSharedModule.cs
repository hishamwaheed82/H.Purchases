using H.Purchases.Localization;
using H.Platform;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace H.Purchases;

[DependsOn(typeof(PlatformDomainSharedModule))]
public class PurchasesDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<PurchasesDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<PurchasesResource>("en")
                .AddVirtualJson("/Localization/Purchases");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Purchases", typeof(PurchasesResource));
        });
    }
}
