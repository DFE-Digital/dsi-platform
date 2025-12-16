namespace Dfe.SignIn.Core.Public.SelectOrganisation;

/// <summary>
/// A model representing an organisation option that can be selected by a user.
/// </summary>
public sealed record SelectOrganisationOption
{
    /// <summary>
    /// The unique identifier of the organisation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The name of the organisation for presentation purposes.
    /// </summary>
    public required string Name { get; init; }
}
