namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// Request to get an organisation by its unique identifier.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetOrganisationByIdResponse"/></item>
///   </list>
///   <para>Throws <see cref="OrganisationNotFoundException"/></para>
///   <list type="bullet">
///     <item>When the organisation was not found.</item>
///   </list>
/// </remarks>
public sealed record GetOrganisationByIdRequest
{
    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetOrganisationByIdRequest"/>.
/// </summary>
public sealed record GetOrganisationByIdResponse
{
    /// <summary>
    /// A model representing the organisation.
    /// </summary>
    public required Organisation Organisation { get; init; }
}
