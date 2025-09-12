using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public.SelectOrganisation;

namespace Dfe.SignIn.Core.Contracts.SelectOrganisation;

/// <summary>
/// Represents a request to filter organisations for a user.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="FilterOrganisationsForUserResponse"/></item>
///   </list>
/// </remarks>
public sealed record FilterOrganisationsForUserRequest
{
    /// <summary>
    /// Gets the unique DfE Sign-in client ID of the application.
    /// </summary>
    [MinLength(1)]
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the organisation filtering specification.
    /// </summary>
    public OrganisationFilter Filter { get; init; } = new OrganisationFilter();
}

/// <summary>
/// Represents a response for <see cref="FilterOrganisationsForUserRequest"/>.
/// </summary>
public sealed record FilterOrganisationsForUserResponse
{
    /// <summary>
    /// Gets the enumerable collection of filtered organisations.
    /// </summary>
    public required IEnumerable<Organisation> FilteredOrganisations { get; init; }
}
