using Dfe.SignIn.NodeApi.Client.HttpSecurityProvider;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

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
