using System.Net;
using System.Net.Http.Json;

namespace Dfe.SignIn.NodeApiClient.AuthenticatedHttpClient;

/// <summary>
/// HttpClient extension methods.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Return a default of T in 404 cases, otherwise return type of T
    /// </summary>
    /// <typeparam name="T">Type definition.</typeparam>
    /// <param name="client">HttpClient instance.</param>
    /// <param name="url">Url to query.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <returns>
    ///   <para>The response object.</para>
    /// </returns>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="HttpRequestException" />
    /// <exception cref="TaskCanceledException" />
    /// <exception cref="UriFormatException" />
    /// <exception cref="OperationCanceledException" />
    public static async Task<T?> GetFromJsonOrDefaultAsync<T>(
        this HttpClient client, string url, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(url);

        return response.StatusCode == HttpStatusCode.NotFound
            ? default
            : await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }
}
