#pragma warning disable CS1591

using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;

namespace Dfe.SignIn.Gateways.BullMq.Models;

/// <summary>
/// Wire-level DTO required by legacy login.dfe.jobs (userUpdatedHandlerV1.js).
/// Note on Edge Cases for 100% Parity:
/// 1. Null properties MUST be omitted from the JSON payload (using JsonIgnoreCondition.WhenWritingNull).
/// 2. `Status` MUST be serialized as an integer/short, matching the DB type.
/// </summary>
public sealed record SafeUserPayload
{
    [JsonPropertyName("sub")]
    public required string Sub { get; init; }

    [JsonPropertyName("given_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GivenName { get; init; }

    [JsonPropertyName("family_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FamilyName { get; init; }

    [JsonPropertyName("email")]
    public required string Email { get; init; }

    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public short? Status { get; init; }

    [JsonPropertyName("legacy_username")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LegacyUsername { get; init; }

    [JsonPropertyName("phone_number")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PhoneNumber { get; init; }

    [JsonPropertyName("last_login")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? LastLogin { get; init; }

    [JsonPropertyName("prev_login")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? PrevLogin { get; init; }

    [JsonPropertyName("isEntra")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsEntra { get; init; }

    [JsonPropertyName("entraOid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EntraOid { get; init; }

    [JsonPropertyName("entraLinked")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? EntraLinked { get; init; }

    [JsonPropertyName("isInternalUser")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsInternalUser { get; init; } = false;

    [JsonPropertyName("entraDeferUntil")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? EntraDeferUntil { get; init; }

    /// <summary>
    /// Creates a new instance of <see cref="SafeUserPayload"/> from a <see cref="UserUpdatedEvent"/>.
    /// </summary>
    /// <param name="event">The domain event containing the updated user information.</param>
    /// <returns>A new instance of <see cref="SafeUserPayload"/>.</returns>
    public static SafeUserPayload FromDomainEvent(UserUpdatedEvent @event) => new() {
        Sub = @event.UserId.ToString(),
        Email = @event.Email,
        GivenName = @event.FirstName,
        FamilyName = @event.LastName,
        Status = @event.Status,
    };
}
