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
public sealed record GetOrganisationsAssociatedWithUserRequest
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetOrganisationsAssociatedWithUserRequest"/>.
/// </summary>
public sealed record GetOrganisationsAssociatedWithUserResponse
{
    /// <summary>
    /// An enumerable collection of organisations that are associated with the user.
    /// </summary>
    public required IEnumerable<Organisation> Organisations { get; init; }
}
