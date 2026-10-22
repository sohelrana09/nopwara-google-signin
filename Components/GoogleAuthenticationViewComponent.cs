using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.ExternalAuth.Google.Components;

/// <summary>
/// Renders the "Continue with Google" button on the login and registration pages
/// </summary>
public class GoogleAuthenticationViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        return View($"~/Plugins/{GoogleAuthenticationDefaults.PLUGIN_FOLDER}/Views/PublicInfo.cshtml");
    }
}
