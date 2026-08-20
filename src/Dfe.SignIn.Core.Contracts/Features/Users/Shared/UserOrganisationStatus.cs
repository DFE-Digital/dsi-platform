namespace Dfe.SignIn.Core.Contracts.Features.Users.Shared;

/// <summary>
/// Represents the status of an organisation user, defined as a smart enum with a unique integer identifier and a display name.
/// </summary>
public sealed record UserOrganisationStatus : SmartEnum<UserOrganisationStatus, short>
{
    /// <inheritdoc/>
    private UserOrganisationStatus(short value, string name) : base(value, name) { }

    /// <summary>
    /// The organisation user status for rejected users (Value = -1, Name = "Rejected").
    /// </summary>
    public static readonly UserOrganisationStatus Rejected = new(-1, "Rejected");

    /// <summary>
    /// The organisation user status for pending users (Value = 0, Name = "Pending
    /// </summary>
    public static readonly UserOrganisationStatus Pending = new(0, "Pending");

    /// <summary>
    /// The organisation user status for approved users (Value = 1, Name = "Approved").
    /// </summary>
    public static readonly UserOrganisationStatus Approved = new(1, "Approved");
}
