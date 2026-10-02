namespace Dfe.SignIn.Web.Profile.Services.AssociatedAccountAuth;

/// <summary>
/// Contains constants used by the <see cref="IAssociatedAccountAuthService"/>.
/// </summary>
public static class AssociatedAccountConstants
{
    /// <summary>
    /// The default Graph API scope used for acquiring delegated access tokens.
    /// </summary>
    public const string DefaultGraphScope = "https://graph.microsoft.com/.default";
}
