namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request to get an organisation by its unique identifier.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetOrganisationByIdResponse"/></item>
///   </list>
/// </remarks>
public sealed record GetOrganisationByIdRequest
{
    /// <summary>
    /// Gets the unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetOrganisationByIdRequest"/>.
/// </summary>
public sealed record GetOrganisationByIdResponse
{
    /// <summary>
    /// Gets a model representing the organisation when the organisation was found;
    /// otherwise, a value of <c>null</c>.
    /// </summary>
    public required Organisation? Organisation { get; init; }
}
