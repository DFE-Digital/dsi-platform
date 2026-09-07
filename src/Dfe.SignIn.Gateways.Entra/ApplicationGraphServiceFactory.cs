using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Dfe.SignIn.Gateways.Entra;

/// <summary>
/// Factory for creating Microsoft Graph service clients authenticated with application credentials.
/// </summary>
public interface IApplicationGraphServiceFactory
{
    /// <summary>
    /// Creates a configured <see cref="GraphServiceClient"/> using daemon / application credentials.
    /// </summary>
    GraphServiceClient CreateClient();
}

/// <summary>
/// Default implementation of <see cref="IApplicationGraphServiceFactory"/> using <see cref="ClientSecretCredential"/>.
/// </summary>
public sealed class ApplicationGraphServiceFactory(
    IOptions<EntraApplicationOptions> options) : IApplicationGraphServiceFactory
{
    private static readonly string[] DefaultScopes = ["https://graph.microsoft.com/.default"];

    /// <inheritdoc/>
    public GraphServiceClient CreateClient()
    {
        var config = options.Value;

        if (string.IsNullOrWhiteSpace(config.TenantId)) {
            throw new InvalidOperationException("Entra TenantId is not configured.");
        }
        if (string.IsNullOrWhiteSpace(config.ClientId)) {
            throw new InvalidOperationException("Entra ClientId is not configured.");
        }
        if (string.IsNullOrWhiteSpace(config.ClientSecret)) {
            throw new InvalidOperationException("Entra ClientSecret is not configured.");
        }

        var credential = new ClientSecretCredential(
            config.TenantId,
            config.ClientId,
            config.ClientSecret);

        return new GraphServiceClient(credential, DefaultScopes);
    }
}
