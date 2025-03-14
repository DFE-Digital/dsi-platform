namespace Dfe.SignIn.NodeApiClient.HttpSecurityProvider;

/// <summary>
/// Represents a IHttpSecurityProvider for authorizing a HttpRequestMessage
/// </summary>
public interface IHttpSecurityProvider
{
    /// <summary>
    /// Adds authorization requirements to the provided HttpRequestMessage
    /// </summary>
    /// <param name="httpRequestMessage"></param>
    /// <returns></returns>
    Task AddAuthorizationAsync(HttpRequestMessage httpRequestMessage);
}
