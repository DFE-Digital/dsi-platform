using Polly;

namespace Dfe.SignIn.InternalApi.Client;

/// <summary>
/// Represents configuration options for an HTTP resilience pipeline.
/// <para>
///   This includes a timeout for requests and retry strategy configuration.
///  These options can be bound from configuration (e.g., appsettings.json) and
///   used when registering resilience pipelines for HTTP clients.
/// </para>
/// </summary>
public sealed class ResilientHttpOptions
{
    /// <summary>
    /// The timeout, in seconds, for HTTP requests when using the associated resilience pipeline.
    /// </summary>
    public int Timeout { get; set; } = 5;

    /// <summary>
    /// Retry options for the HTTP pipeline.
    /// </summary>
    public ResilientHttpRetryOptions Retry { get; set; } = new();
}

/// <summary>
/// Represents the retry configuration for a resilient HTTP pipeline.
/// </summary>
public sealed class ResilientHttpRetryOptions
{
    /// <summary>
    /// The maximum number of retry attempts for a failed request.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// The delay between retry attempts, in milliseconds.
    /// </summary>
    public int Delay { get; set; } = 200;

    /// <summary>
    /// Indicates whether to apply jitter to the retry delay.
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// The backoff strategy to use for retries <see cref="DelayBackoffType"/>.
    /// </summary>
    public DelayBackoffType BackoffType { get; set; } = DelayBackoffType.Exponential;
}
