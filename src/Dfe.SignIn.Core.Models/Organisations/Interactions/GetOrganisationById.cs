namespace Dfe.SignIn.Core.Models.Organisations.Interactions;

/// <summary>
/// Request to get an organisation by its unique identifier.
/// </summary>
public record GetOrganisationByIdRequest()
{
    /// <summary>
    /// Gets the unique identifier of the organisation.
    /// </summary>
    public required Guid OrganisationId { get; init; }
}

/// <summary>
/// Response model for request <see cref="GetOrganisationByIdRequest"/>.
/// </summary>
public record GetOrganisationByIdResponse()
{
    /// <summary>
    /// Gets a model representing the organisation when the organisation was found;
    /// otherwise, a value of <c>null</c>.
    /// </summary>
    public required OrganisationModel? Organisation { get; init; }
}
