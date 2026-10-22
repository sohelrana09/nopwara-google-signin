using Nop.Core.Configuration;

namespace Nop.Plugin.ExternalAuth.Google;

/// <summary>
/// Represents the plugin settings
/// </summary>
public class GoogleExternalAuthSettings : ISettings
{
    /// <summary>
    /// Gets or sets the OAuth 2.0 client ID (Google Cloud console > APIs & Services > Credentials)
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the OAuth 2.0 client secret
    /// </summary>
    public string ClientSecret { get; set; }
}
