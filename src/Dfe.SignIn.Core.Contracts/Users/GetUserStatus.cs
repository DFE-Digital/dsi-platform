using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework.DataAnnotations;

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
public sealed record GetUserStatusRequest
{
    /// <summary>
    /// Gets the unique ID of the user in the Entra tenant.
    /// </summary>
    [RequiredIfTargetEquals(nameof(EmailAddress), null!)]
    public Guid? EntraUserId { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [RequiredIfTargetEquals(nameof(EntraUserId), null!)]
    [EmailAddress]
    public string? EmailAddress { get; init; }
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
    /// Gets the unique ID of the user.
    /// </summary>
    /// <remarks>
    ///   <para>Has a value of <c>null</c> when <see cref="UserExists"/> is <c>false</c>.</para>
    /// </remarks>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Gets a value indicating the status of the user account.
    /// </summary>
    /// <remarks>
    ///   <para>Has a value of <c>null</c> when <see cref="UserExists"/> is <c>false</c>.</para>
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
