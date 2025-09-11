using Dfe.SignIn.Core.Contracts.Organisations;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Request to get all of the organisations that are associated with a particular user.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetOrganisationsAssociatedWithUserResponse"/></item>
///   </list>
/// </remarks>
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
    public required IEnumerable<Organisation> Organisations { get; init; }
}
