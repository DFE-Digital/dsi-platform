namespace Dfe.SignIn.Core.ExternalModels.Organisations;

/// <summary>
/// Provides details about an organisation.
/// </summary>
public record OrganisationDetails()
{
    /// <summary>
    /// Gets the unique value that identifies the organisation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the organisation.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the legal name of the organisation.
    /// </summary>
    public string? LegalName { get; init; }

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
    //   address - Non-structured address.
    //   telephone - Non-structured phone number.
    //   statutoryLowAge - Lower age of student in organisation if applicable.
    //   statutoryHighAge - Higher age of student in organisation if applicable.
    //   companyRegistrationNumber - As per companies house.
    //   isOnApar - Indicates whether the organisation is on the apprenticeship register.
    //   legacyId - A unique ID from an older version of the system.
}
