using Dfe.SignIn.Core.InternalModels.Organisations;

namespace Dfe.SignIn.Core.InternalModels.Users.Interactions;

/// <summary>
/// Request to get all of the organisations that are associated with a particular user.
/// </summary>
public record GetOrganisationsAssociatedWithUserRequest()
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetOrganisationsAssociatedWithUserRequest"/>.
/// </summary>
public record GetOrganisationsAssociatedWithUserResponse()
{
    /// <summary>
    /// Gets a list of zero-or-more models representing the organisations that are
    /// associated with a particular user.
    /// </summary>
    public required IEnumerable<OrganisationModel> Organisations { get; init; }
}
