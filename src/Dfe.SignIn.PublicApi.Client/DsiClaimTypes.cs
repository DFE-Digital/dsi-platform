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
    internal const string SessionId = "sid";

    /// <summary>
    /// Gets the name of the DfE Sign-in user ID claim type.
    /// </summary>
    public const string UserId = "dsi_user_id";

    /// <summary>
    /// Gets the name of the DfE Sign-in organisation claim type.
    /// </summary>
    public const string Organisation = "organisation";

    /// <summary>
    /// Gets the "roleid" claim type.
    /// </summary>
    public const string RoleId = "roleid";

    /// <summary>
    /// Gets the "rolename" claim type.
    /// </summary>
    public const string RoleName = "rolename";

    /// <summary>
    /// Gets the "rolecode" claim type.
    /// </summary>
    public const string RoleCode = "rolecode";

    /// <summary>
    /// Gets the "rolenumericid" claim type.
    /// </summary>
    public const string RoleNumericId = "rolenumericid";
}
