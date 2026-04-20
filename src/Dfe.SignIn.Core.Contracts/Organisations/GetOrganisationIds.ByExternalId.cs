using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request to get all organisations ids matching the UKPRN or UPIM .
/// </summary>
[AssociatedResponse(typeof(GetOrganisationIdsByExternalIdResponse))]
public sealed record GetOrganisationIdsByExternalIdRequest
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
/// Response model for request <see cref="GetOrganisationIdsByExternalIdRequest"/>.
/// </summary>
public sealed record GetOrganisationIdsByExternalIdResponse
{
    /// <summary>
    /// An enumerable collection of organisations ids.
    /// </summary>
    public required IEnumerable<Guid> OrganisationIds { get; init; }
}
