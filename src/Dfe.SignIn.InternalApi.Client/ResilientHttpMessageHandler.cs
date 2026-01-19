using Polly;
using Polly.Registry;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// A <see cref="DelegatingHandler"/> that applies resilience policies to outgoing HTTP requests.
/// <para>
///   This handler integrates with the Microsoft Resilience pipeline system to apply
///   timeouts, retries, and other resilience strategies on a per-request or per-client basis.
/// </para>
/// </summary>
/// <param name="resiliencePipelineProvider">
/// Provides access to the registered resilience pipelines. Used to resolve a pipeline by name for a specific HTTP request.
///</param>
/// <param name="resilientHttpMessageHandlerOptions">
/// Options for the handler, including the default pipeline name.
/// </param>
public sealed class ResilientHttpMessageHandler(
    ResiliencePipelineProvider<string> resiliencePipelineProvider,
    ResilientHttpMessageHandlerOptions resilientHttpMessageHandlerOptions
) : DelegatingHandler
{

    /// <summary>
    /// Sends an HTTP request applying the configured resilience policies.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete</param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var (pipeline, strategyName) = this.ResolvePipelineStrategyOrDefault(request);

        request.Options.Set(ResilientHttpMessageHandlerOptions.UsedResiliencePipeline, strategyName);
        return await pipeline.ExecuteAsync(async token => await base.SendAsync(request, token), cancellationToken);
    }

    /// <summary>
    /// Resolves the <see cref="ResiliencePipeline{HttpResponseMessage}"/> to use for the given request.
    /// </summary>
    /// <param name="request">The HTTP request message for which to resolve the pipeline.</param>
    /// <returns><para>The resolved <see cref="ResiliencePipeline{HttpResponseMessage}"/> to execute.</para></returns>
    private (ResiliencePipeline<HttpResponseMessage> pipeline, string strategyName) ResolvePipelineStrategyOrDefault(HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(ResilientHttpMessageHandlerOptions.RequestedResiliencePipeline, out var pipelineStrategyName)) {
            if (resiliencePipelineProvider.TryGetPipeline<HttpResponseMessage>(pipelineStrategyName, out var resiliencePipeline)) {
                return (resiliencePipeline, pipelineStrategyName);
            }
        }

        return (
            resiliencePipelineProvider.GetPipeline<HttpResponseMessage>(resilientHttpMessageHandlerOptions.DefaultStrategyName),
            resilientHttpMessageHandlerOptions.DefaultStrategyName);
    }
}
