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
    /// See <see cref="SelectOrganisationCallbackId"/>.
    /// </summary>
    public const string Id = "id";

    /// <summary>
    /// See <see cref="SelectOrganisationCallbackBasic"/>.
    /// </summary>
    public const string Basic = "basic";

    /// <summary>
    /// See <see cref="SelectOrganisationCallbackExtended"/>.
    /// </summary>
    public const string Extended = "extended";

    /// <summary>
    /// See <see cref="SelectOrganisationCallbackLegacy"/>.
    /// </summary>
    public const string Legacy = "legacy";
}
