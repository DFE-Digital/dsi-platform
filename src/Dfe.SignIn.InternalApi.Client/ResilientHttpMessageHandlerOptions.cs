namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// Options for configuring a <see cref="ResilientHttpMessageHandler"/>.
/// </summary>
/// <remarks>
///   <para> Contains the name of the default resilience strategy to use when a request
///   does not specify a pipeline override, and exposes the key used to store
///   per-request pipeline overrides in <see cref="HttpRequestMessage.Options"/>.</para>
/// </remarks>
public sealed class ResilientHttpMessageHandlerOptions
{
    /// <summary>
    /// The <see cref="HttpRequestOptionsKey{TValue}"/> used to store or retrieve
    /// a per-request resilience pipeline override in <see cref="HttpRequestMessage.Options"/>.
    /// </summary>
    /// <remarks>
    ///   <para>Used to indicate which ResiliencePipeline was requested to be used for executing a request.</para>
    /// </remarks>
    public static HttpRequestOptionsKey<string> RequestedResiliencePipeline { get; } = new("DsiNameOfRequestedResiliencePipeline");

    /// <summary>
    /// The <see cref="HttpRequestOptionsKey{TValue}"/> used to store or retrieve
    /// a per-request resilience pipeline override in <see cref="HttpRequestMessage.Options"/>.
    /// </summary>
    /// <remarks>
    ///   <para>Used to indicate which ResiliencePipeline was used when executing a request.</para>
    /// </remarks>
    public static HttpRequestOptionsKey<string> UsedResiliencePipeline { get; } = new("DsiNameOfUsedResiliencePipeline");
}
