using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;

/// <summary>
/// Represents a request to get "select organisation" session data.
/// </summary>
public sealed record GetSelectOrganisationSessionByKeyRequest
{
    /// <summary>
    /// Gets the unique key of the session.
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
    /// Gets the "select organisation" session data if any was found.
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
