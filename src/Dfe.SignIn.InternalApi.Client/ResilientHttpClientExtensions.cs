
using System.Net.Http.Json;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// Extension methods for <see cref="HttpClient" /> to send requests with resilience
/// policies applied via <see cref="ResilientHttpMessageHandler"/>.
/// </summary>
public static class ResilientHttpClientExtensions
{
    /// <summary>
    /// Sends an HTTP request with JSON content using the specified HTTP method,
    /// applying the specified resilience pipeline.
    /// </summary>
    /// <typeparam name="TRequest">Type of the content to serialize as JSON. Can be null for no body.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/> to send the request.</param>
    /// <param name="httpMethod">The HTTP method requested.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The content to send as JSON. Ignored if null.</param>
    /// <param name="resiliencePipelineName">The resilience pipeline to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><para>The <see cref="HttpResponseMessage"/> from the request.</para></returns>
    public static async Task<HttpResponseMessage> SendAsJsonAsync<TRequest>(
        this HttpClient httpClient,
        HttpMethod httpMethod,
        string requestUri,
        TRequest content,
        string? resiliencePipelineName,
        CancellationToken cancellationToken = default)
    {

        var httpRequest = new HttpRequestMessage(httpMethod, requestUri);

        if (content is not null) {
            httpRequest.Content = JsonContent.Create(content);
        }

        if (!string.IsNullOrEmpty(resiliencePipelineName)) {
            httpRequest.Options.Set(ResilientHttpMessageHandlerOptions.RequestedResiliencePipeline, resiliencePipelineName);
        }

        return await httpClient.SendAsync(httpRequest, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP request with JSON content using the specified HTTP method,
    /// applying a resilience pipeline derived from the request content type.
    /// </summary>
    /// <remarks>
    ///   <para>This overload automatically selects the resilience pipeline by using
    ///   <c>typeof(TValue).Name</c> as the pipeline name. This allows request-specific
    ///   resilience strategies to be configured without explicitly specifying
    ///   a pipeline name at the call site.</para>
    /// </remarks>
    /// <typeparam name="TRequest">Type of the content to serialize as JSON. Can be null for no body.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/> to send the request.</param>
    /// <param name="httpMethod">The HTTP method requested.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="content">The content to send as JSON. Ignored if null.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><para>The <see cref="HttpResponseMessage"/> from the request.</para></returns>
    public static async Task<HttpResponseMessage> SendAsJsonAsync<TRequest>(
        this HttpClient httpClient,
        HttpMethod httpMethod,
        string requestUri,
        TRequest content,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.SendAsJsonAsync(httpMethod, requestUri, content, content?.GetType().Name, cancellationToken);
    }
}
