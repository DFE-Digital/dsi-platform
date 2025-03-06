namespace Dfe.SignIn.Core.Models.Organisations;

/// <summary>
/// A model representing an organisation in DfE Sign-in.
/// </summary>
public sealed record OrganisationModel
{
    /// <summary>
    /// Gets the unique value that identifies the organisation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the name of the organisation.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the legal name of the organisation.
    /// </summary>
    public required string LegalName { get; init; }
}
