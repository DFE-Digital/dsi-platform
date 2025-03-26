namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// A service representing the DfE Sign-in public API client.
/// </summary>
public interface IPublicApiClient
{
    /// <summary>
    /// Gets the associated HTTP client which can be used to make requests directly to
    /// the DfE Sign-in public API.
    /// </summary>
    HttpClient HttpClient { get; }
}

/// <summary>
/// Concrete implementation of <see cref="IPublicApiClient"/>.
/// </summary>
/// <param name="httpClient">The underlying HTTP client for the DfE Sign-in public API.</param>
internal sealed class PublicApiClient(
    [FromKeyedServices(PublicApiConstants.HttpClientKey)] HttpClient httpClient
) : IPublicApiClient
{
    /// <inheritdoc/>
    public HttpClient HttpClient => httpClient;
}
