using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Users;

/// <summary>
/// Represents a request to complete any pending invitation associated with a user.
/// </summary>
[AssociatedResponse(typeof(CompleteAnyPendingInvitationResponse))]
public sealed record CompleteAnyPendingInvitationRequest
{
    /// <summary>
    /// The email address of the user.
    /// </summary>
    /// <value>
    /// A well formed email address.
    /// </value>
    [RegularExpression(StringPatterns.EmailAddressPattern)]
    public required string EmailAddress { get; init; }

    /// <summary>
    /// The unique ID of the user in the Entra tenant.
    /// </summary>
    public required Guid EntraUserId { get; init; }
}

/// <summary>
/// Represents a response for <see cref="CompleteAnyPendingInvitationRequest"/>.
/// </summary>
public sealed record CompleteAnyPendingInvitationResponse
{
    /// <summary>
    /// A value indicating if an existing invitation was completed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(UserId))]
    public required bool WasCompleted { get; init; }

    /// <summary>
    /// The unique ID of the user in DfE Sign-in.
    /// </summary>
    /// <remarks>
    ///   <para>Has a value of null if <see cref="WasCompleted"/> is false.</para>
    /// </remarks>
    public Guid? UserId { get; init; }
}
