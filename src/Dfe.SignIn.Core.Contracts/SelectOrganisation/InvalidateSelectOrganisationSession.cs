using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.SelectOrganisation;

/// <summary>
/// Represents a request to invalidate a "select organisation" session.
/// </summary>
[AssociatedResponse(typeof(InvalidateSelectOrganisationSessionResponse))]
public sealed record InvalidateSelectOrganisationSessionRequest
{
    /// <summary>
    /// The unique key of the session.
    /// </summary>
    [MinLength(1)]
    public required string SessionKey { get; init; }
}

/// <summary>
/// Represents a response for <see cref="InvalidateSelectOrganisationSessionRequest"/>.
/// </summary>
public sealed record InvalidateSelectOrganisationSessionResponse
{
}
