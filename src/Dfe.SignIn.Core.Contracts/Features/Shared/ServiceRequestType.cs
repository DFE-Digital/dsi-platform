namespace Dfe.SignIn.Core.Contracts.Features.Shared;

/// <summary>
/// Represents the type of service request, defined as a smart enum with a unique short identifier and a display name.
/// </summary>
public sealed record ServiceRequestType : SmartEnum<ServiceRequestType, string>
{
    /// <inheritdoc/>
    private ServiceRequestType(string value, string name) : base(value, name) { }

    /// <summary>
    /// The service request type for service access requests (Value = "service", Name = "Service access").
    /// </summary>
    public static readonly ServiceRequestType Service = new("service", "Service access");

    /// <summary>
    /// The service request type for sub-service access requests (Value = "subService", Name = "Sub-service access").
    /// </summary>
    public static readonly ServiceRequestType SubService = new("subService", "Sub-service access");
}
