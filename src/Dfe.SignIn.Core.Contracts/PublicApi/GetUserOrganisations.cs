using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;

namespace Dfe.SignIn.Core.Contracts.PublicApi;

/// <summary>
/// Request to get the list of organisations a user belongs to.
/// Hidden organisations (status = 0) are filtered out.
/// </summary>
[AssociatedResponse(typeof(GetUserOrganisationsResponse))]
public sealed record GetUserOrganisationsRequest
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetUserOrganisationsRequest"/>.
/// </summary>
public sealed record GetUserOrganisationsResponse
{
    /// <summary>
    /// The visible organisations that the user belongs to.
    /// </summary>
    public required IEnumerable<Organisation> Organisations { get; init; }
}
