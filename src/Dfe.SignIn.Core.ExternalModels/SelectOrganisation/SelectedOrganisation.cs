namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when a
/// detail level of <see cref="OrganisationDetailLevel.Id"/> is specified.
/// </summary>
public record SelectedOrganisation()
{
    private static readonly Dictionary<OrganisationDetailLevel, Type> TypeMappings = new() {
        { OrganisationDetailLevel.Id, typeof(SelectedOrganisation) },
        { OrganisationDetailLevel.Basic, typeof(SelectedOrganisationBasic) },
        { OrganisationDetailLevel.Extended, typeof(SelectedOrganisationExtended) },
        { OrganisationDetailLevel.Legacy, typeof(SelectedOrganisationLegacy) },
    };

    /// <summary>
    /// Tries to resolve the <see cref="SelectedOrganisation"/> type for the given detail level.
    /// </summary>
    /// <param name="detailLevel">The detail level of the selection.</param>
    /// <returns>
    ///   <para>The resolved type of <see cref="SelectedOrganisation"/>; otherwise,
    ///   a value of <c>null</c>.</para>
    /// </returns>
    public static Type? TryResolveType(OrganisationDetailLevel detailLevel)
    {
        TypeMappings.TryGetValue(detailLevel, out var result);
        return result;
    }

    /// <summary>
    /// Resolves the <see cref="SelectedOrganisation"/> type for the given detail level.
    /// </summary>
    /// <param name="detailLevel">The detail level of the selection.</param>
    /// <returns>
    ///   <para>The resolved type of <see cref="SelectedOrganisation"/>.</para>
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///   <para>If type could not be resolved for <paramref name="detailLevel"/>.</para>
    /// </exception>
    public static Type ResolveType(OrganisationDetailLevel detailLevel)
    {
        return TryResolveType(detailLevel)
            ?? throw new InvalidOperationException($"Cannot resolve unknown type '{detailLevel}'.");
    }

    /// <summary>
    /// Gets the unique value that identifies the organisation.
    /// </summary>
    public required Guid Id { get; init; }
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when a
/// detail level of <see cref="OrganisationDetailLevel.Basic"/> is specified.
/// </summary>
public record SelectedOrganisationBasic()
    : SelectedOrganisation
{
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
}

/// <summary>
/// The type of data payload supplied to the "select organisation" callback when a
/// detail level of <see cref="OrganisationDetailLevel.Extended"/> is specified.
/// </summary>
public record SelectedOrganisationExtended()
    : SelectedOrganisationBasic
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
public record SelectedOrganisationLegacy()
    : SelectedOrganisationExtended
{
    // TODO: Add missing properties...
    //   legacyId - A unique ID from an older version of the system.
}
