namespace Dfe.SignIn.Core.Contracts.Access;

/// <summary>
/// Request to get the roles that are associated with an application.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetRolesOfApplicationResponse"/></item>
///   </list>
/// </remarks>
public sealed record GetRolesOfApplicationRequest
{
    /// <summary>
    /// The unique identifier of the application.
    /// </summary>
    public required Guid ApplicationId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetRolesOfApplicationRequest"/>.
/// </summary>
public sealed record GetRolesOfApplicationResponse
{
    /// <summary>
    /// An enumerable collection of application roles.
    /// </summary>
    public required IEnumerable<ApplicationRole> Roles { get; init; }
}
