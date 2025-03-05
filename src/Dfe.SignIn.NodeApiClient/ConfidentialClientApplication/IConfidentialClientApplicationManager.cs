
namespace Dfe.SignIn.NodeApiClient.ConfidentialClientApplication;

/// <summary>
/// Represents a ConfidentialClientApplicationManager
/// </summary>
public interface IConfidentialClientApplicationManager
{
    /// <summary>
    /// For a given NodeApiName and HttpRequestMessage add the necessary authorization attributes
    /// </summary>
    /// <param name="nodeApiName">The NodeApiName for the authorization attributes to be used.</param>
    /// <param name="httpRequestMessage">The HttpRequestMessage to which authorization attributes will be applied</param>
    /// <returns></returns>
    Task AddAuthorizationAsync(NodeApiName nodeApiName, HttpRequestMessage httpRequestMessage);
}