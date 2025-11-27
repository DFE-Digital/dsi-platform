using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to complete any pending invitation associated with a user.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="CompleteAnyPendingInvitationResponse"/></item>
///   </list>
/// </remarks>
public sealed record CompleteAnyPendingInvitationRequest
{
    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public required string EmailAddress { get; init; }

    /// <summary>
    /// Gets the unique ID of the user in the Entra tenant.
    /// </summary>
    public required Guid EntraUserId { get; init; }
}

/// <summary>
/// Represents a response for <see cref="CompleteAnyPendingInvitationRequest"/>.
/// </summary>
public sealed record CompleteAnyPendingInvitationResponse
{
    /// <summary>
    /// Gets a value indicating if an existing invitation was completed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(UserId))]
    public required bool WasCompleted { get; init; }

    /// <summary>
    /// Gets the unique ID of the user in DfE Sign-in.
    /// </summary>
    /// <remarks>
    ///   <para>Has a value of <c>null</c> if <see cref="WasCompleted"/> is <c>false</c>.</para>
    /// </remarks>
    public Guid? UserId { get; init; }
}
