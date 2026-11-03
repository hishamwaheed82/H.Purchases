using H.Purchases.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace H.Purchases;

public abstract class PurchasesController : AbpControllerBase
{
    protected PurchasesController()
    {
        LocalizationResource = typeof(PurchasesResource);
    }
}
