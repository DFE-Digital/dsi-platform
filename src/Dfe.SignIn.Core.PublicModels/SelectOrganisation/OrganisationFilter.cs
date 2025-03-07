namespace Dfe.SignIn.Core.PublicModels.SelectOrganisation;

/// <summary>
/// A model representing how organisations are to be filtered in the user interface.
/// </summary>
public sealed record OrganisationFilter()
{
    /// <summary>
    /// Gets a value specifying the type of filtering to be used.
    /// </summary>
    public OrganisationFilterType Type { get; init; } = OrganisationFilterType.Associated;

    /// <summary>
    /// Gets a value specifying the type of association to be used when filtering.
    /// </summary>
    public OrganisationFilterAssociation Association { get; init; } = OrganisationFilterAssociation.Auto;

    /// <summary>
    /// Gets the list of unique organisation identifiers to specify when applicable.
    /// </summary>
    public IEnumerable<Guid> OrganisationIds { get; init; } = [];
}

/// <summary>
/// Represents a type of filtering that can be applied to a list of organisations.
/// </summary>
public enum OrganisationFilterType
{
    /// <summary>
    /// Organisations that are associated with the user.
    /// </summary>
    /// <remarks>
    ///   <para><see cref="OrganisationFilter.OrganisationIds"/> is not applicable
    ///   when this association type is being used.</para>
    /// </remarks>
    Associated = 0,

    /// <summary>
    /// Organisations that are associated with the user but only including those that
    /// have been explicitly specified.
    /// </summary>
    /// <remarks>
    ///   <para>Use <see cref="OrganisationFilter.OrganisationIds"/> to specify which
    ///   organisations are to be included.</para>
    /// </remarks>
    AssociatedInclude = 1,

    /// <summary>
    /// Organisations that are associated with the user but excluding any that have
    /// been explicitly specified.
    /// </summary>
    /// <remarks>
    ///   <para>Use <see cref="OrganisationFilter.OrganisationIds"/> to specify which
    ///   organisations are to be excluded.</para>
    /// </remarks>
    AssociatedExclude = 2,

    /// <summary>
    /// Any of the organisations that have been explicitly specified regardless of
    /// whether they are associated with the user.
    /// </summary>
    /// <remarks>
    ///   <para>Use <see cref="OrganisationFilter.OrganisationIds"/> to specify the
    ///   exact list of organisations are to be listed regardless of whether they
    ///   are associated with the user.</para>
    /// </remarks>
    AnyOf = 3,
}

/// <summary>
/// Represents the type of association when applying filtering to organisations.
/// </summary>
public enum OrganisationFilterAssociation
{
    /// <summary>
    /// Behaves as <see cref="AssignedToUser"/> when the service is ID-only; otherwise,
    /// behaves as <see cref="AssignedToUserForService"/> for a role-based aservice.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Only include organisations that are assigned to the user.
    /// </summary>
    AssignedToUser = 1,

    /// <summary>
    /// Only include organisations that are assigned to the user where the user
    /// has permissions to interact with that service.
    /// </summary>
    AssignedToUseForService = 2,
}
