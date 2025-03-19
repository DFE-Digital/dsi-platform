using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Core.Models.Applications;

/// <summary>
/// The exception thrown when an application cannot be found.
/// </summary>
public sealed class ApplicationNotFoundException : InteractionException
{
    /// <inheritdoc/>
    public ApplicationNotFoundException() { }

    /// <inheritdoc/>
    public ApplicationNotFoundException(string? message)
        : base(message) { }

    /// <inheritdoc/>
    public ApplicationNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }

    /// <inheritdoc/>
    public ApplicationNotFoundException(string? message, Guid applicationId)
        : base(message)
    {
        this.ApplicationId = applicationId;
    }

    /// <inheritdoc/>
    public ApplicationNotFoundException(string? message, string clientId)
        : base(message)
    {
        this.ClientId = clientId;
    }

    /// <summary>
    /// Gets the unique ID of the application that was requested.
    /// </summary>
    [Persist]
    public Guid? ApplicationId { get; private set; }

    /// <summary>
    /// Gets the unique client ID of the application that was requested.
    /// </summary>
    [Persist]
    public string? ClientId { get; private set; }
}
