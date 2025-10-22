namespace Dfe.SignIn.Web.Profile;

/// <summary>
/// Key constants that can be used when accessing <see cref="HttpContext.Session"/> data.
/// </summary>
public static class SessionKeys
{
    /// <summary>
    /// Key for a value indicating whether the current user is an internal team member.
    /// </summary>
    /// <remarks>
    ///   <para>A value of 1 indicates that the user is an internal team member.</para>
    ///   <para>A value of 0 indicates that the user is external.</para>
    /// </remarks>
    public const string IsInternalUser = "5949f39e-9d69-4c78-8512-df198c4c7dbc";

    #region Hybrid integration

    /// <summary>
    /// Key for hybrid integration code verifier.
    /// </summary>
    public const string HybridCodeVerifier = "125097a0-2ec3-492d-b0db-6bf390bad109";

    /// <summary>
    /// Key for hybrid integration access token which can be used with the Graph API.
    /// </summary>
    public const string HybridAccessToken = "8e64adac-b146-461a-9d76-bfeaaa43d1e6";

    /// <summary>
    /// Key for hybrid integration access token expiry which can be used with the Graph API.
    /// </summary>
    public const string HybridAccessTokenExpiresOn = "9f261193-4b1c-4625-8711-26d2e023e7bb";

    /// <summary>
    /// Key for hybrid integration return URI during authentication.
    /// </summary>
    public const string HybridReturnUri = "15500975-23cd-46dd-a3ef-a5106cdeb7b9";

    #endregion
}
