using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to get the status of a user account.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetUserStatusResponse"/></item>
///   </list>
/// </remarks>
public sealed record GetUserStatusRequest : IValidatableObject
{
    /// <summary>
    /// The unique ID of the user in the Entra tenant.
    /// </summary>
    public Guid? EntraUserId { get; init; }

    /// <summary>
    /// The email address of the user.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public string? EmailAddress { get; init; }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        string[] memberNames = [
            nameof(this.EntraUserId),
            nameof(this.EmailAddress)
        ];
        if (!ValidationHelpers.HasExactlyOneMember(validationContext, memberNames)) {
            yield return new ValidationResult("Exactly one option must be specified.", memberNames);
        }
    }
}

/// <summary>
/// Represents a response for <see cref="GetUserStatusRequest"/>.
/// </summary>
public sealed record GetUserStatusResponse
{
    /// <summary>
    /// Gets a value indicating if the user account actually exists.
    /// </summary>
    [MemberNotNullWhen(true, nameof(UserId))]
    [MemberNotNullWhen(true, nameof(AccountStatus))]
    public required bool UserExists { get; init; }

    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    /// <remarks>
    ///   <para>Has a value of null when <see cref="UserExists"/> is false.</para>
    /// </remarks>
    public Guid? UserId { get; init; }

    /// <summary>
    /// A value indicating the status of the user account.
    /// </summary>
    /// <remarks>
    ///   <para>Has a value of null when <see cref="UserExists"/> is false.</para>
    /// </remarks>
    public AccountStatus? AccountStatus { get; init; }
}

/// <summary>
/// Represents the status of a user account in DfE Sign-in.
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// Indicates that the user account is inactive.
    /// </summary>
    Inactive = 0,

    /// <summary>
    /// Indicates that the user account is active.
    /// </summary>
    Active = 1,
}
