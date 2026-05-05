using System.Text.Json.Serialization;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request and response models for getting users at an organisation.
/// </summary>
/// <param name="ExternalId"></param>
[AssociatedResponse(typeof(GetUsersAtOrganisationResponseNew))]
public record GetUsersAtOrganisationRequestNew(
    string ClientId,
    string ExternalId
);

/// <summary>
/// Represents the response containing users associated with an organisation.
/// </summary>
public class GetUsersAtOrganisationResponseNew
{
    /// <summary>
    /// True if a UKPRN is being expressed, false if UPIN is being shown.
    /// </summary>
    [JsonIgnore]
    public bool IsUkprn { get; set; }

    /// <summary>
    /// The value of the UKPRN or UPIN.
    /// </summary>
    [JsonIgnore]
    public string? ExternalId { get; set; }

    /// <summary>
    /// Ukprn label.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Ukprn => this.IsUkprn ? this.ExternalId : null;

    /// <summary>
    /// Upin label
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Upin => !this.IsUkprn ? this.ExternalId : null;
    public IReadOnlyList<UserAtOrganisationNew>? Users { get; set; }
}

/// <summary>
/// Represents a user associated with an organisation.
/// </summary>
/// <param name="Email">
/// The user's email address.
/// </param>
/// <param name="FirstName">
/// The user's first name.
/// </param>
/// <param name="LastName">
/// The user's last name.
/// </param>
/// <param name="UserStatus">
/// The numeric status code representing the user's current state.
/// </param>
/// <param name="Role">
/// A read-only list of roles assigned to the user within the organisation.
/// </param>
public record UserAtOrganisationNew(
    Guid Sub,
    string Email,
    string FirstName,
    string LastName,
    int UserStatus,
    string? Role);
