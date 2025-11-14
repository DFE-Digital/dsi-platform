namespace Dfe.SignIn.Core.Contracts.Audit;

/// <summary>
/// Request to write an audit entry, including metadata and contextual information.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="WriteToAuditResponse"/></item>
///   </list>
/// </remarks>
public sealed record WriteToAuditRequest
{
    /// <summary>
    /// The category of the audit event.
    /// </summary>
    /// <remarks>
    ///   <para>Maps to 'type' in legacy system.</para>
    /// </remarks>
    public required string EventCategory { get; init; }

    /// <summary>
    /// The name of the audit event.
    /// </summary>
    /// <remarks>
    ///   <para>Maps to 'sub-type' in legacy system.</para>
    ///   <para>Some audit events do not currently specify a 'sub-type'.</para>
    /// </remarks>
    public string? EventName { get; init; }

    /// <summary>
    /// A descriptive message explaining the audit event.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Explicitly specifies the ID of the user associated with the event.
    /// </summary>
    /// <remarks>
    ///   <para>This property is generally not needed since it is automatically inferred
    ///   from the current request context.</para>
    /// </remarks>
    public Guid? UserId { get; init; }

    /// <summary>
    /// The ID of the organisation associated with the event, if applicable.
    /// </summary>
    public Guid? OrganisationId { get; init; }

    /// <summary>
    /// A collection of custom key-value pairs to include in the audit metadata.
    /// </summary>
    public IEnumerable<KeyValuePair<string, object>> CustomProperties { get; init; } = [];
}

/// <summary>
/// Response model for interactor <see cref="WriteToAuditRequest"/>.
/// </summary>
public sealed record WriteToAuditResponse
{
}
