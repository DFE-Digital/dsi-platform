namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// Indicates the type of error that has occurred.
/// </summary>
public enum SelectOrganisationErrorCode
{
    /// <summary>
    /// Indicates that an internal error has occurred.
    /// </summary>
    InternalError = 0,

    /// <summary>
    /// Indicates that an invalid selection was made.
    /// </summary>
    InvalidSelection = 1,

    /// <summary>
    /// Indicates that there were no options for the user to choose from.
    /// </summary>
    NoOptions = 2,
}
