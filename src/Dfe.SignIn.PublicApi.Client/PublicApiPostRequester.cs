using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// General implementation of an interactor that makes POST requests to Dfe Sign-in
/// public API endpoints.
/// </summary>
/// <param name="client">The API client service.</param>
/// <param name="jsonOptions">JSON serializer options.</param>
/// <param name="endpoint">The endpoint (eg. "v2/select-organisation").</param>
[ExcludeFromCodeCoverage]
internal class PublicApiPostRequester<TRequest, TResponse>(
    IPublicApiClient client,
    JsonSerializerOptions jsonOptions,
    string endpoint
) : IInteractor<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    // This class is excluded from coverage testing since extension functions like
    // `HttpClient.GetFromJsonAsync` are difficult to mock. It would be good to
    // revisit the unit testing of this code module in the future.

    /// <inheritdoc/>
    public async Task<TResponse> InvokeAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        var httpClient = client.HttpClient;

        var httpResponse = await httpClient.PostAsJsonAsync(
            this.TransformEndpoint(request, endpoint),
            request,
            jsonOptions,
            cancellationToken
        );

        httpResponse.EnsureSuccessStatusCode();

        return await httpResponse.Content.ReadFromJsonAsync<TResponse>(
            jsonOptions, cancellationToken
        ) ?? throw new MissingResponseDataException();
    }

    /// <summary>
    /// Transforms endpoint from request parameters.
    /// </summary>
    /// <param name="request">The interaction request.</param>
    /// <param name="endpoint">The endpoint (eg. "v2/select-organisation").</param>
    /// <returns>
    ///   <para>The transformed endpoint.</para>
    /// </returns>
    protected virtual string TransformEndpoint(TRequest request, string endpoint)
    {
        return endpoint;
    }
}
