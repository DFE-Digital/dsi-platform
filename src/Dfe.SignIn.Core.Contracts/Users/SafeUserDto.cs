using System.Text.Json.Serialization;

namespace Dfe.SignIn.Core.Contracts.Users;

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
