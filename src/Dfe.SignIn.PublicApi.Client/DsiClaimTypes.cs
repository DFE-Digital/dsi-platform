namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// DfE Sign-in claim type name constants.
/// </summary>
/// <seealso cref="DsiClaimExtensions"/>
public static class DsiClaimTypes
{
    /// <summary>
    /// Gets the name of the session ID claim type.
    /// </summary>
    public const string SessionId = "sid";

    /// <summary>
    /// Gets the name of the DfE Sign-in user ID claim type.
    /// </summary>
    public const string UserId = "dsi_user_id";
}
