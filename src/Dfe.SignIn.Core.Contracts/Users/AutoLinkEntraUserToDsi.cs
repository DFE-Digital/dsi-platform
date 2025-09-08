using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to automatically link an Entra user to DfE Sign-in.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="AutoLinkEntraUserToDsiResponse"/></item>
///   </list>
///   <para>Throws <see cref="CannotLinkInactiveUserException"/></para>
///   <list type="bullet">
///     <item>When attempting to link an inactive user account.</item>
///   </list>
/// </remarks>
public sealed record AutoLinkEntraUserToDsiRequest
{
    /// <summary>
    /// Gets the unique ID of the user in the Entra tenant.
    /// </summary>
    public required Guid EntraUserId { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [EmailAddress]
    public required string EmailAddress { get; init; }

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
/// Represents a response for <see cref="AutoLinkEntraUserToDsiRequest"/>.
/// </summary>
public sealed record AutoLinkEntraUserToDsiResponse
{
    /// <summary>
    /// Gets the unique ID of the user in DfE Sign-in.
    /// </summary>
    public Guid UserId { get; set; }
}
