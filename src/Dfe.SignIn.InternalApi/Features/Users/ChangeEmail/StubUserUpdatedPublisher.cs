using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Interfaces.Notifications;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;

//todo: this is just a placeholder for now, we will implement this properly when we have an notification service to send messages to. For now, we just log the call to this method.

/// <summary>
/// A no-op implementation of <see cref="IUserUpdatedPublisher"/> for presentation use.
/// </summary>
public sealed class StubUserUpdatedPublisher(ILogger<StubUserUpdatedPublisher> logger) : IUserUpdatedPublisher
{
    /// <inheritdoc/>
    public Task PublishUserUpdatedAsync(Guid userId, string emailAddress, string firstName, string lastName, short status, CancellationToken cancellationToken)
    {
        var safeUserDto = new SafeUserDto(
            Sub: userId.ToString(),
            GivenName: firstName,
            FamilyName: lastName,
            Email: emailAddress,
            JobTitle: null,
            Id: userId.ToString(),
            Status: status.ToString(),
            LegacyUsername: null,
            PhoneNumber: null,
            LastLogin: null,
            PrevLogin: null,
            IsEntra: null,
            EntraOid: null,
            EntraLinked: null,
            IsInternalUser: null,
            EntraDeferUntil: null
        );

        logger.LogInformation("StubUserUpdatedPublisher: PublishUserUpdatedAsync called for user {UserId} with email {Email}. SafeUserDto: {@SafeUserDto}", userId, emailAddress, safeUserDto);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Represents a safe user data transfer object (DTO) that contains user information.
/// This is a stop gap to allow the internal API to communicate with redis queue
/// </summary>
/// <param name="Sub"></param>
/// <param name="GivenName"></param>
/// <param name="FamilyName"></param>
/// <param name="Email"></param>
/// <param name="JobTitle"></param>
/// <param name="Id"></param>
/// <param name="Status"></param>
/// <param name="LegacyUsername"></param>
/// <param name="PhoneNumber"></param>
/// <param name="LastLogin"></param>
/// <param name="PrevLogin"></param>
/// <param name="IsEntra"></param>
/// <param name="EntraOid"></param>
/// <param name="EntraLinked"></param>
/// <param name="IsInternalUser"></param>
/// <param name="EntraDeferUntil"></param>
public record SafeUserDto(
    [property: JsonPropertyName("sub")] string? Sub,
    [property: JsonPropertyName("given_name")] string? GivenName,
    [property: JsonPropertyName("family_name")] string? FamilyName,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("job_title")] string? JobTitle,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("legacy_username")] string? LegacyUsername,
    [property: JsonPropertyName("phone_number")] string? PhoneNumber,
    [property: JsonPropertyName("last_login")] DateTimeOffset? LastLogin,
    [property: JsonPropertyName("prev_login")] DateTimeOffset? PrevLogin,
    [property: JsonPropertyName("isEntra")] bool? IsEntra,
    [property: JsonPropertyName("entraOid")] string? EntraOid,
    [property: JsonPropertyName("entraLinked")] bool? EntraLinked,
    [property: JsonPropertyName("isInternalUser")] bool? IsInternalUser,
    [property: JsonPropertyName("entraDeferUntil")] DateTimeOffset? EntraDeferUntil
);
