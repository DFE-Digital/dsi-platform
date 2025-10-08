namespace Dfe.SignIn.PublicApi.Authorization;

/// <summary>
/// Provides information about a scoped session which is useful when handling
/// a Public API request.
/// </summary>
public interface IClientSession
{
    /// <summary>
    /// Gets the unique client ID of the service application.
    /// </summary>
    string ClientId { get; }
}

/// <summary>
/// Represents a service that sets up a scoped session which is then used when
/// handling a Public API request.
/// </summary>
public interface IClientSessionWriter
{
    /// <summary>
    /// Sets the unique client ID of the service application.
    /// </summary>
    string ClientId { set; }
}

/// <summary>
/// Scoped session provider implementation
/// </summary>
public sealed class ClientSessionProvider : IClientSession, IClientSessionWriter
{
    /// <inheritdoc/>
    public string ClientId { get; set; } = default!;
}
