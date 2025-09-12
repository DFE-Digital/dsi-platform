using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to link an Entra user to DfE Sign-in.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="LinkEntraUserToDsiResponse"/></item>
///   </list>
/// </remarks>
public sealed record LinkEntraUserToDsiRequest
{
    /// <summary>
    /// Gets the unique ID of the user in DfE Sign-in.
    /// </summary>
    public required Guid DsiUserId { get; init; }

    /// <summary>
    /// Gets the unique ID of the user in the Entra tenant.
    /// </summary>
    public required Guid EntraUserId { get; init; }

    /// <summary>
    /// Gets the given name of the user.
    /// </summary>
    [MinLength(1)]
    public required string GivenName { get; init; }

    /// <summary>
    /// Gets the surname of the user.
    /// </summary>
    [MinLength(1)]
    public required string Surname { get; init; }
}

/// <summary>
/// Represents a response for <see cref="LinkEntraUserToDsiRequest"/>.
/// </summary>
public sealed record LinkEntraUserToDsiResponse
{
}
