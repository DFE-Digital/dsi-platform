namespace Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

/// <summary>
/// A model representing an organisation option that can be selected by a user.
/// </summary>
public sealed record SelectOrganisationOption()
{
    /// <summary>
    /// Gets the unique identifier of the organisation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the organisation for presentation purposes.
    /// </summary>
    public required string Name { get; init; }
}
