namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Models;

/// <summary>
/// 
/// </summary>
public sealed class User
{
    /// <summary>
    /// 
    /// </summary>
    public required string Username { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required string FirstName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required string LastName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public required Guid EntraOid { get; set; }
}
