using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Organisations;

/// <summary>
/// The exception thrown when a requested organisation was not found.
/// </summary>
public sealed class OrganisationNotFoundException : InteractionException
{
    /// <summary>
    /// Creates an instance of the <see cref="OrganisationNotFoundException"/> from an organisation ID.
    /// </summary>
    /// <param name="organisationId">ID of the organisation.</param>
    public static OrganisationNotFoundException FromOrganisationId(Guid organisationId)
        => new() { OrganisationId = organisationId };

    /// <inheritdoc/>
    public OrganisationNotFoundException() { }

    /// <inheritdoc/>
    public OrganisationNotFoundException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public OrganisationNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Gets or sets the ID of the organisation that was not found.
    /// </summary>
    [Persist]
    public Guid? OrganisationId { get; private set; }
}
