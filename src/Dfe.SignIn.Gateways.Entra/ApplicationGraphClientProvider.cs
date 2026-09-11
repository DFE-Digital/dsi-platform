using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Dfe.SignIn.Gateways.Entra;

/// <summary>
/// Provides a shared Microsoft Graph service client authenticated with application credentials.
/// </summary>
public interface IApplicationGraphClientProvider
{
    /// <summary>
    /// Gets a configured <see cref="GraphServiceClient"/> using daemon / application credentials.
    /// </summary>
    GraphServiceClient GetClient();
}

/// <summary>
/// Default implementation of <see cref="IApplicationGraphClientProvider"/> using <see cref="ClientSecretCredential"/>.
/// </summary>
public sealed class ApplicationGraphClientProvider(
    IOptions<EntraApplicationSettings> options) : IApplicationGraphClientProvider
{
    private static readonly string[] DefaultScopes = ["https://graph.microsoft.com/.default"];
    private readonly Lazy<GraphServiceClient> client = new(() => CreateGraphClient(options));

    /// <inheritdoc/>
    public GraphServiceClient GetClient() => this.client.Value;

    private static GraphServiceClient CreateGraphClient(IOptions<EntraApplicationSettings> options)
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
