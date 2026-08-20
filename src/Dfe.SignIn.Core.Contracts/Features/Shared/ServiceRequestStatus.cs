namespace Dfe.SignIn.Core.Contracts.Features.Shared;

/// <summary>
/// Represents the status of a service request, defined as a smart enum with a unique integer identifier and a display name.
/// </summary>
public sealed record ServiceRequestStatus : SmartEnum<ServiceRequestStatus, short>
{
    /// <inheritdoc/>
    private ServiceRequestStatus(short value, string name) : base(value, name) { }

    /// <summary>
    /// The service request status indicating that the request has been rejected (Value = -1, Name = "Rejected").
    /// </summary>
    public static readonly ServiceRequestStatus Rejected = new(-1, "Rejected");

    /// <summary>
    /// The service request status indicating that the request is pending (Value = 0, Name = "Pending").
    /// </summary>
    public static readonly ServiceRequestStatus Pending = new(0, "Pending");

    /// <summary>
    /// The service request status indicating that the request has been approved (Value = 1, Name = "Approved").
    /// </summary>
    public static readonly ServiceRequestStatus Approved = new(1, "Approved");

    /// <summary>
    /// The service request status indicating that the request is overdue (Value = 2, Name = "Overdue").
    /// </summary>
    public static readonly ServiceRequestStatus Overdue = new(2, "Overdue");

    /// <summary>
    /// The service request status indicating that there are no approvers available for the request (Value = 3, Name = "No Approvers").
    /// </summary>
    public static readonly ServiceRequestStatus NoApprovers = new(3, "No Approvers");
}
