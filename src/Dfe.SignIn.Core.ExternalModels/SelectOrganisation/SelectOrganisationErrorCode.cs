namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// Constant values representing the different error codes resulting for a failed
/// "select organisation" callback.
/// </summary>
public static class SelectOrganisationErrorCode
{
    /// <summary>
    /// Indicates that an internal error has occurred.
    /// </summary>
    public const string InternalError = "internalError";

    /// <summary>
    /// Indicates that an invalid selection was made.
    /// </summary>
    public const string InvalidSelection = "invalidSelection";

    /// <summary>
    /// Indicates that there were no options for the user to choose from.
    /// </summary>
    public const string NoOptions = "noOptions";
}
