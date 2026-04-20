using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request to get all user belogning to an organisations.
/// </summary>
[AssociatedResponse(typeof(GetOrganisationIdsResponse))]
public sealed record GetOrganisationIdsRequest
{
    /// <summary>
    /// Key, identifies which search to perform.
    /// </summary>
    public required string LookupKey { get; init; }

    /// <summary>
    /// UKPRN or UPIN of the organisation.
    /// </summary>
    public required string LookupValue { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetOrganisationIdsRequest"/>.
/// </summary>
public sealed record GetOrganisationIdsResponse
{
    /// <summary>
    /// An enumerable collection of organisations ids.
    /// </summary>
    public required IEnumerable<Guid> OrganisationIds { get; init; }
}
