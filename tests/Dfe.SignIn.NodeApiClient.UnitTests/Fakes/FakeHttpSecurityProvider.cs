using Dfe.SignIn.NodeApiClient.HttpSecurityProvider;

namespace Dfe.SignIn.NodeApiClient.UnitTests.Fakes;

/// <summary>
/// Is a fake implementation of IHttpSecurityProvider for testing purposes.
/// </summary>
internal class FakeHttpSecurityProvider : IHttpSecurityProvider
{
    public Task AddAuthorizationAsync(
        HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken = default)
    {
        httpRequestMessage.Headers.Authorization = new("Bearer", "fake-bearer-token");
        return Task.CompletedTask;
    }
}
