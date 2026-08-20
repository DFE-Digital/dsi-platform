namespace Dfe.SignIn.Core.Contracts.Features.Users.Shared;

/// <summary>
/// Represents the status of an organisation request, defined as a smart enum with a unique integer identifier and a display name.
/// </summary>
public sealed record OrganisationRequestStatus : SmartEnum<OrganisationRequestStatus, short>
{
    /// <inheritdoc/>
    private OrganisationRequestStatus(short value, string name) : base(value, name) { }

    /// <summary>
    /// The organisation request status indicating that the request has been rejected (Value = -1, Name = "Rejected").
    /// </summary>
    public static readonly OrganisationRequestStatus Rejected = new(-1, "Rejected");

    /// <summary>
    /// The organisation request status indicating that the request is pending (Value = 0, Name = "Pending").
    /// </summary>
    public static readonly OrganisationRequestStatus Pending = new(0, "Pending");

    /// <summary>
    /// The organisation request status indicating that the request has been approved (Value = 1, Name = "Approved").
    /// </summary>
    public static readonly OrganisationRequestStatus Approved = new(1, "Approved");

    /// <summary>
    /// The organisation request status indicating that the request is overdue (Value = 2, Name = "Overdue").
    /// </summary>
    public static readonly OrganisationRequestStatus Overdue = new(2, "Overdue");

    /// <summary>
    /// The organisation request status indicating that there are no approvers for the request (Value = 3, Name = "No Approvers").
    /// </summary>
    public static readonly OrganisationRequestStatus NoApprovers = new(3, "No Approvers");
}
