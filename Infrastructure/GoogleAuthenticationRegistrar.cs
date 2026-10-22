using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication.External;

namespace Nop.Plugin.ExternalAuth.Google.Infrastructure;

/// <summary>
/// Registers the Google authentication handler (found by core through IExternalAuthenticationRegistrar)
/// </summary>
public class GoogleAuthenticationRegistrar : IExternalAuthenticationRegistrar
{
    public void Configure(AuthenticationBuilder builder)
    {
        builder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            //placeholders keep the handler valid until the plugin is configured (the login action refuses to start then)
            var settings = EngineContext.Current.Resolve<GoogleExternalAuthSettings>();
            options.ClientId = string.IsNullOrEmpty(settings?.ClientId) ? nameof(options.ClientId) : settings.ClientId;
            options.ClientSecret = string.IsNullOrEmpty(settings?.ClientSecret) ? nameof(options.ClientSecret) : settings.ClientSecret;
            options.CallbackPath = GoogleAuthenticationDefaults.CALLBACK_PATH;

            //openid + profile + email are the defaults; let the customer pick an account every time
            options.SaveTokens = true;

            //not mapped by default: the callback refuses addresses Google hasn't verified
            options.ClaimActions.MapJsonKey(GoogleAuthenticationDefaults.EMAIL_VERIFIED_CLAIM, "email_verified");
            options.Events = new OAuthEvents
            {
                OnRedirectToAuthorizationEndpoint = context =>
                {
                    context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                    return Task.CompletedTask;
                },

                //on error or cancel, go back to the login page instead of an exception page
                OnRemoteFailure = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect(context.Properties?.GetString(GoogleAuthenticationDefaults.ERROR_CALLBACK) ?? "/login");
                    return Task.CompletedTask;
                }
            };
        });
    }
}
