namespace Dfe.SignIn.Web.Profile;

/// <summary>
/// Constant values for use with the external identity provider.
/// </summary>
public static class ExternalAuthConstants
{
    /// <summary>
    /// The name of the external Open ID connect scheme.
    /// </summary>
    public const string OpenIdConnectSchemeName = "ExternalScheme";

    /// <summary>
    /// The name of the external cookies scheme.
    /// </summary>
    public const string CookiesSchemeName = "ExternalCookies";

    /// <summary>
    /// The name of the external auth cookie.
    /// </summary>
    public const string CookieName = "ExternalAuth";
}
