using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// Represents callback data for a "select organisation" submission.
/// </summary>
public abstract record SelectOrganisationCallback()
{
    /// <summary>
    /// Gets type type of callback payload.
    /// </summary>
    [MinLength(1)]
    public required string Type { get; init; }
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when an
/// error has occurred.
/// </summary>
public record SelectOrganisationCallbackError() : SelectOrganisationCallback
{
    /// <summary>
    /// Gets a value indicating the kind of error that has occurred.
    /// </summary>
    [EnumDataType(typeof(SelectOrganisationErrorCode))]
    public required SelectOrganisationErrorCode Code { get; init; }
}

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

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when a
/// detail level of <see cref="OrganisationDetailLevel.Id"/> is specified.
/// </summary>
public record SelectOrganisationCallbackId() : SelectOrganisationCallback
{
    /// <summary>
    /// Gets the unique value that identifies the organisation.
    /// </summary>
    public required Guid Id { get; init; }
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when a
/// detail level of <see cref="OrganisationDetailLevel.Basic"/> is specified.
/// </summary>
public record SelectOrganisationCallbackBasic()
    : SelectOrganisationCallbackId
{
    /// <summary>
    /// Gets the name of the organisation.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the legal name of the organisation.
    /// </summary>
    public required string LegalName { get; init; }

    // TODO: Add missing properties...
    //   category - Category of the organisation.
    //   type - Type of organisation.
    //   urn - Unique reference number.
    //   uid - Unique identifier.
    //   upin - Unique provider identification number.
    //   ukprn - UK provider reference number.
    //   establishmentNumber - Code that identifies which establishment.
    //   localAuthority - Local authority if applicable.
    //   status - Status of the organisation.
    //   closedOn - Indicates when the organisation closed.
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when a
/// detail level of <see cref="OrganisationDetailLevel.Extended"/> is specified.
/// </summary>
public record SelectOrganisationCallbackExtended()
    : SelectOrganisationCallbackBasic
{
    // TODO: Add missing properties...
    //   address - Non-structured address.
    //   telephone - Non-structured phone number.
    //   statutoryLowAge - Lower age of student in organisation if applicable.
    //   statutoryHighAge - Higher age of student in organisation if applicable.
    //   companyRegistrationNumber - As per companies house.
    //   isOnApar - Indicates whether the organisation is on the apprenticeship register.
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when a
/// detail level of <see cref="OrganisationDetailLevel.Legacy"/> is specified.
/// </summary>
public record SelectOrganisationCallbackLegacy()
    : SelectOrganisationCallbackExtended
{
    // TODO: Add missing properties...
    //   legacyId - A unique ID from an older version of the system.
}
