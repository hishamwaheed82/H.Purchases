using H.Platform;
using H.Purchases.Gl;
using Microsoft.Extensions.DependencyInjection;
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
        var configuration = context.Services.GetConfiguration();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<PurchasesApplicationModule>(validate: false);
        });

        // ── Purchases → GL options ────────────────────────────────────────────
        context.Services.Configure<PurchasesGlOptions>(configuration.GetSection(PurchasesGlOptions.SectionName));

        // ── Shared HttpClients ────────────────────────────────────────────────
        context.Services.AddHttpClient("H.Auth");
        context.Services.AddHttpClient("H.Accounting.GL");

        // ── Purchases → GL services ───────────────────────────────────────────
        context.Services.AddSingleton<AccountingGlClient>();
        context.Services.AddScoped<PurchasesGlPoster>();

        context.Services.AddMemoryCache();
    }
}
