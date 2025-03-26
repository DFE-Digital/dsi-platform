using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// General implementation of an interactor that makes GET requests to Dfe Sign-in
/// public API endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class PublicApiGetRequester<TRequest, TResponse>(
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
    public async Task<TResponse> InvokeAsync(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var httpClient = client.HttpClient;

        return await httpClient.GetFromJsonAsync<TResponse>(endpoint, jsonOptions)
            ?? throw new MissingResponseDataException();
    }
}
