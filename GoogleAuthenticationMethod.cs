using Nop.Core;
using Nop.Services.Helpers;
using Nop.Plugin.ExternalAuth.Google.Components;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.ExternalAuth.Google;

/// <summary>
/// Represents the Google external authentication method
/// </summary>
public class GoogleAuthenticationMethod : BasePlugin, IExternalAuthenticationMethod
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public GoogleAuthenticationMethod(ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/GoogleAuthentication/Configure";
    }

    /// <summary>
    /// Gets a type of a view component for displaying plugin in public store
    /// </summary>
    public Type GetPublicViewComponent()
    {
        return typeof(GoogleAuthenticationViewComponent);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new GoogleExternalAuthSettings());

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.ExternalAuth.Google.ClientId"] = "Client ID",
            ["Plugins.ExternalAuth.Google.ClientId.Hint"] = "The OAuth 2.0 client ID from the Google Cloud console.",
            ["Plugins.ExternalAuth.Google.ClientSecret"] = "Client secret",
            ["Plugins.ExternalAuth.Google.ClientSecret.Hint"] = "The OAuth 2.0 client secret from the Google Cloud console.",
            ["Plugins.ExternalAuth.Google.RedirectUri"] = "Authorized redirect URI",
            ["Plugins.ExternalAuth.Google.Instructions"] = "<p>To let customers sign in with Google:</p><ol><li>Open the <a href=\"https://console.cloud.google.com/apis/credentials\" target=\"_blank\" rel=\"noopener\">Google Cloud console &gt; APIs &amp; Services &gt; Credentials</a> (create a project if needed).</li><li>Configure the <b>OAuth consent screen</b> (app name, support email, scopes <code>openid</code>, <code>email</code>, <code>profile</code>).</li><li>Click <b>Create credentials &gt; OAuth client ID</b>, choose <b>Web application</b>.</li><li>Add the <b>Authorized redirect URI</b> shown below.</li><li>Copy the client ID and client secret here, save, then activate the method in Configuration &gt; Authentication &gt; External authentication.</li></ol>",
            ["Plugins.ExternalAuth.Google.Button"] = "Continue with Google",
            ["Plugins.ExternalAuth.Google.EmailNotVerified"] = "Your Google account email address is not verified. Verify it with Google, or register with your email address."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<GoogleExternalAuthSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.ExternalAuth.Google");

        await base.UninstallAsync();
    }

    #endregion
}
