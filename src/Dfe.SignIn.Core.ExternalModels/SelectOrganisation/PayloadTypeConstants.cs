namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// Constant values indicating the type of "select organisation" callback payload types.
/// </summary>
/// <seealso cref="SelectOrganisationCallback.TryResolveType(string)" />
/// <seealso cref="SelectOrganisationCallback.ResolveType(string)" />
public static class PayloadTypeConstants
{
    /// <summary>
    /// See <see cref="SelectOrganisationCallbackError"/>.
    /// </summary>
    public const string Error = "error";

    /// <summary>
    /// See <see cref="SelectOrganisationCallbackSignOut"/>.
    /// </summary>
    public const string SignOut = "signOut";

    /// <summary>
    /// See <see cref="SelectOrganisationCallbackCancel"/>.
    /// </summary>
    public const string Cancel = "cancel";

    /// <summary>
    /// See <see cref="SelectOrganisationCallbackSelection"/>.
    /// </summary>
    public const string Selection = "selection";
}
