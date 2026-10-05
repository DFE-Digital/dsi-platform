using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Models;

/// <summary>
/// Encapsulates the user identity information required to create
/// a new DSI user from an Entra ID account.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class UserCreationDto
{
    /// <summary>
    /// Given name of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// FirstName associated to the user
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Surname of the user.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Unique object identifier of the user in Entra ID.
    /// </summary>
    public required Guid EntraOid { get; set; }
}
