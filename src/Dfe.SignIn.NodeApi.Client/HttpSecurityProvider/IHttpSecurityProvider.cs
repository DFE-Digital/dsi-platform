namespace Dfe.SignIn.NodeApi.Client.HttpSecurityProvider;

/// <summary>
/// Represents a IHttpSecurityProvider for authorizing a HttpRequestMessage
/// </summary>
public interface IHttpSecurityProvider
{
    /// <summary>
    /// Adds authorization requirements to the provided HttpRequestMessage
    /// </summary>
    /// <param name="httpRequestMessage"></param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="OperationCanceledException" />
    Task AddAuthorizationAsync(
        HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken = default
    );
}
