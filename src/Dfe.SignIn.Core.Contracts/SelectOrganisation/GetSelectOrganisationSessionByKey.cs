using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.SelectOrganisation;

/// <summary>
/// Represents a request to get "select organisation" session data.
/// </summary>
[AssociatedResponse(typeof(GetSelectOrganisationSessionByKeyResponse))]
public sealed record GetSelectOrganisationSessionByKeyRequest
{
    /// <summary>
    /// The unique key of the session.
    /// </summary>
    [MinLength(1)]
    public required string SessionKey { get; init; }
}

/// <summary>
/// Represents a response for <see cref="GetSelectOrganisationSessionByKeyRequest"/>.
/// </summary>
public sealed record GetSelectOrganisationSessionByKeyResponse
{
    /// <summary>
    /// The "select organisation" session data if any was found.
    /// </summary>
    /// <remarks>
    ///   <para>Session data will not be found when:</para>
    ///   <list type="bullet">
    ///     <item>The session does not exist.</item>
    ///     <item>The session has expired.</item>
    ///   </list>
    /// </remarks>
    public SelectOrganisationSessionData? SessionData { get; init; }
}
