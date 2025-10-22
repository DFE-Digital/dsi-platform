using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to change the email address of a user.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="ChangeEmailAddressResponse"/></item>
///   </list>
///   <para>Throws <see cref="UserNotFoundException"/></para>
///   <list type="bullet">
///     <item>When the user account was not found.</item>
///   </list>
/// </remarks>
public sealed record ChangeEmailAddressRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>
    /// The user's new email address.
    /// </summary>
    [Required(ErrorMessage = "Enter an email address")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public required string NewEmailAddress { get; init; }
}

/// <summary>
/// Represents a response for <see cref="ChangeEmailAddressRequest"/>.
/// </summary>
public sealed record ChangeEmailAddressResponse
{
}
