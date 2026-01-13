
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// Extension methods to configure HTTP clients for Node API services with
/// Microsoft Resilience pipelines and Polly-based retry strategies.
/// </summary>
public static class ResilientServiceCollectionExtensions
{
    /// <summary>
    /// Configures HTTP clients for the specified httpClientNames to use
    /// a resilient message handler and registers named resilience pipelines
    /// based on the provided configuration section.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which HTTP clients and pipelines are added.</param>
    /// <param name="httpClientNames">A collection of string values representing already registered HttpClients.</param>
    /// <param name="configurationRoot">The configuration root.</param>
    /// <param name="defaultStrategyName">The name of the default resilience pipeline to use.</param>
    /// <returns><para>The updated <see cref="IServiceCollection"/>.</para></returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="services"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="configurationRoot"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// <para>Thrown if the <paramref name="configurationRoot"/> does not contain
    /// any resilience strategies.</para>
    /// </exception>
    public static IServiceCollection SetupResilientHttpClient(
        this IServiceCollection services,
        IEnumerable<string> httpClientNames,
        IConfigurationRoot configurationRoot,
        string defaultStrategyName)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));
        ExceptionHelpers.ThrowIfArgumentNull(configurationRoot, nameof(configurationRoot));
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(defaultStrategyName, nameof(defaultStrategyName));

        services.AddTransient<ResilientHttpMessageHandler>();

        var resilienceStrategy = configurationRoot.GetJson<Dictionary<string, ResilientHttpOptions>>("HttpResiliency")
            ?? throw new InvalidOperationException("HttpResiliency configuration section is empty.");

        foreach (var apiName in httpClientNames) {
            services.AddHttpClient(apiName).AddHttpMessageHandler<ResilientHttpMessageHandler>();
        }

        foreach (var (strategyName, strategyOptions) in resilienceStrategy) {
            services.AddResiliencePipeline<string, HttpResponseMessage>(strategyName, builder => {
                builder.AddTimeout(TimeSpan.FromSeconds(strategyOptions.Timeout));
                builder.AddRetry(RetryPredicates.DefaultHttpRetryOptions(strategyOptions.Retry));
            });
        }

        services.AddSingleton(new ResilientHttpMessageHandlerOptions {
            DefaultStrategyName = defaultStrategyName
        });

        return services;
    }

    /// <summary>
    /// Helper class that provides default Polly retry options and predicates for HTTP requests.
    /// </summary>
    private static class RetryPredicates
    {
        /// <summary>
        /// Builds an <see cref="HttpRetryStrategyOptions"/> object using the supplied
        /// <see cref="ResilientHttpRetryOptions"/>.
        /// </summary>
        /// <param name="options">The retry configuration options.</param>
        /// <returns><para>A fully configured <see cref="HttpRetryStrategyOptions"/> instance.</para></returns>
        public static HttpRetryStrategyOptions DefaultHttpRetryOptions(ResilientHttpRetryOptions options)
        {
            return new HttpRetryStrategyOptions {
                MaxRetryAttempts = options.MaxRetryAttempts,
                BackoffType = options.BackoffType,
                UseJitter = options.UseJitter,
                Delay = TimeSpan.FromMilliseconds(options.Delay),
                ShouldHandle = DefaultHttpRetryPredicate()
            };
        }

        /// <summary>
        /// Defines the default retry predicate for HTTP requests.
        /// </summary>
        /// <returns><para>A <see cref="PredicateBuilder{HttpResponseMessage}"/> configured for default retry conditions.</para></returns>
        private static PredicateBuilder<HttpResponseMessage> DefaultHttpRetryPredicate()
        {
            return new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(r => (int)r.StatusCode >= 500);
        }
    }
}
