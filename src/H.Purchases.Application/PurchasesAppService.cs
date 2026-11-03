using H.Purchases.Localization;
using Volo.Abp.Application.Services;

namespace H.Purchases;

public abstract class PurchasesAppService : ApplicationService
{
    protected PurchasesAppService()
    {
        LocalizationResource = typeof(PurchasesResource);
    }
}
