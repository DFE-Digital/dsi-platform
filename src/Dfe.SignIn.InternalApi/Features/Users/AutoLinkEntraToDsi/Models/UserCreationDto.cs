namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Models;

/// <summary>
/// A class representing a  user creation domain object
/// </summary>
public sealed class UserCreationDto
{
    /// <summary>
    /// Username associated to the user
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// FirstName associated to the user
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// LastName associated to the user
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// EntraOid associated to the user
    /// </summary>
    public required Guid EntraOid { get; set; }
}
