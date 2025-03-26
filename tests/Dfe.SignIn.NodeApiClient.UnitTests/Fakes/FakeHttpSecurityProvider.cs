using Dfe.SignIn.NodeApiClient.HttpSecurityProvider;

namespace Dfe.SignIn.NodeApiClient.UnitTests.Fakes;

/// <summary>
/// Is a fake implementation of IHttpSecurityProvider for testing purposes.
/// </summary>
internal class FakeHttpSecurityProvider : IHttpSecurityProvider
{
    public Task AddAuthorizationAsync(HttpRequestMessage httpRequestMessage)
    {
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "fake-bearer-token");
        return Task.CompletedTask;
    }
}
