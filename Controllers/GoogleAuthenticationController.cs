using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nop.Core;
using Nop.Services.Helpers;
using Nop.Core.Http;
using Nop.Plugin.ExternalAuth.Google.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.ExternalAuth.Google.Controllers;

[AutoValidateAntiforgeryToken]
public class GoogleAuthenticationController : BasePluginController
{
    #region Fields

    private readonly GoogleExternalAuthSettings _settings;
    private readonly IAuthenticationPluginManager _authenticationPluginManager;
    private readonly IExternalAuthenticationService _externalAuthenticationService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IOptionsMonitorCache<GoogleOptions> _optionsCache;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public GoogleAuthenticationController(GoogleExternalAuthSettings settings,
        IAuthenticationPluginManager authenticationPluginManager,
        IExternalAuthenticationService externalAuthenticationService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IOptionsMonitorCache<GoogleOptions> optionsCache,
        ISettingService settingService,
        IStoreContext storeContext,
        IWebHelper webHelper,
        IWorkContext workContext)
    {
        _settings = settings;
        _authenticationPluginManager = authenticationPluginManager;
        _externalAuthenticationService = externalAuthenticationService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _optionsCache = optionsCache;
        _settingService = settingService;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _workContext = workContext;
    }

    #endregion

    #region Admin

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_EXTERNAL_AUTHENTICATION_METHODS)]
    public IActionResult Configure()
    {
        var model = new ConfigurationModel
        {
            ClientId = _settings.ClientId,
            ClientSecret = _settings.ClientSecret,
            RedirectUri = _webHelper.GetStoreLocation().TrimEnd('/') + GoogleAuthenticationDefaults.CALLBACK_PATH
        };

        return View($"~/Plugins/{GoogleAuthenticationDefaults.PLUGIN_FOLDER}/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [CheckPermission(StandardPermission.Configuration.MANAGE_EXTERNAL_AUTHENTICATION_METHODS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return Configure();

        _settings.ClientId = model.ClientId?.Trim();
        _settings.ClientSecret = model.ClientSecret?.Trim();
        await _settingService.SaveSettingAsync(_settings);

        //the handler options are built once per scheme, so drop them to pick up the new credentials
        _optionsCache.TryRemove(GoogleDefaults.AuthenticationScheme);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return Configure();
    }

    #endregion

    #region Storefront

    public async Task<IActionResult> Login(string returnUrl)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var available = await _authenticationPluginManager
            .IsPluginActiveAsync(GoogleAuthenticationDefaults.SYSTEM_NAME, await _workContext.GetCurrentCustomerAsync(), store.Id);
        if (!available)
            throw new NopException("Google authentication module cannot be loaded");

        if (string.IsNullOrEmpty(_settings.ClientId) || string.IsNullOrEmpty(_settings.ClientSecret))
            throw new NopException("Google authentication module not configured");

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("LoginCallback", "GoogleAuthentication", new { returnUrl })
        };
        properties.SetString(GoogleAuthenticationDefaults.ERROR_CALLBACK, Url.RouteUrl(NopRouteNames.General.LOGIN, new { returnUrl }));

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> LoginCallback(string returnUrl)
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded || !result.Principal.Claims.Any())
            return RedirectToRoute(NopRouteNames.General.LOGIN);

        //an unverified email could be someone else's address
        var emailVerified = result.Principal.FindFirst(GoogleAuthenticationDefaults.EMAIL_VERIFIED_CLAIM)?.Value;
        if (emailVerified != null && !string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.ExternalAuth.Google.EmailNotVerified"));
            return RedirectToRoute(NopRouteNames.General.LOGIN, new { returnUrl });
        }

        var parameters = new ExternalAuthenticationParameters
        {
            ProviderSystemName = GoogleAuthenticationDefaults.SYSTEM_NAME,
            AccessToken = await HttpContext.GetTokenAsync(GoogleDefaults.AuthenticationScheme, "access_token"),
            Email = result.Principal.FindFirst(claim => claim.Type == ClaimTypes.Email)?.Value,
            ExternalIdentifier = result.Principal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
            ExternalDisplayIdentifier = result.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name)?.Value,
            Claims = result.Principal.Claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList()
        };

        //core signs in the customer associated with this Google account, or registers a new one (an existing email is refused, never linked)
        return await _externalAuthenticationService.AuthenticateAsync(parameters, returnUrl);
    }

    #endregion
}
