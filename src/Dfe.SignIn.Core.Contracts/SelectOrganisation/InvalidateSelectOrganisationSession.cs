using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.SelectOrganisation;

/// <summary>
/// Represents a request to invalidate a "select organisation" session.
/// </summary>
/// <remarks>
///   <para>Does nothing if the session does not exist.</para>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="InvalidateSelectOrganisationSessionResponse"/></item>
///   </list>
/// </remarks>
public sealed record InvalidateSelectOrganisationSessionRequest
{
    /// <summary>
    /// Gets the unique key of the session.
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
