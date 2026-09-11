namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Models;

/// <summary>
/// A class representing a basic user domain object
/// </summary>
public sealed class User
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
