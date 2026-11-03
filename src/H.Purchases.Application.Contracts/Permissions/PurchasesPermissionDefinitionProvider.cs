using H.Purchases.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace H.Purchases.Permissions;

/// <summary>
/// SoftPlan catalog already registers Purchases.* screen actions.
/// Keep this empty to avoid duplicate permission name collisions.
/// </summary>
public class PurchasesPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        // no-op — SoftPlanPermissionDefinitionProvider owns Purchases.* names
    }

    private static LocalizableString L(string name) =>
        LocalizableString.Create<PurchasesResource>(name);
}
