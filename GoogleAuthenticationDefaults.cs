namespace Nop.Plugin.ExternalAuth.Google;

/// <summary>
/// Represents plugin constants
/// </summary>
public static class GoogleAuthenticationDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public const string SYSTEM_NAME = "ExternalAuth.Google";

    /// <summary>
    /// Gets the folder (under Nop.Web/Plugins) the plugin is deployed to
    /// </summary>
    public const string PLUGIN_FOLDER = "ExternalAuth.Google";

    /// <summary>
    /// Gets the name of the authentication property holding the URL to return to when Google sign-in fails or is cancelled
    /// </summary>
    public const string ERROR_CALLBACK = "ErrorCallback";

    /// <summary>
    /// Gets the claim type holding Google's "email_verified" flag
    /// </summary>
    public const string EMAIL_VERIFIED_CLAIM = "urn:google:email_verified";

    /// <summary>
    /// Gets the callback path registered in the Google Cloud console (Authorized redirect URI = store URL + this path)
    /// </summary>
    public const string CALLBACK_PATH = "/signin-google";
}
