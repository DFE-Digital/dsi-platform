using System.Collections.Concurrent;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.TestHelpers.Integration.Mocks;

public sealed class FakeInteractionLimiter : IInteractionLimiter
{
    private readonly ConcurrentDictionary<string, int> counts = new();

    /// <summary>
    /// Gets or sets the threshold at which actions will be rejected. 
    /// Defaults to 3 to mirror the real limiter's default.
    /// </summary>
    public int MaxAllowedInteractions { get; set; } = 3;

    /// <summary>
    /// Gets or sets a global toggle to force all requests to be rejected or accepted.
    /// Set to true to immediately simulate a rate-limit breach.
    /// </summary>
    public bool ShouldAlwaysReject { get; set; }

    /// <inheritdoc/>
    public Task<InteractionLimiterResult> LimitActionAsync(IKeyedRequest request, string? key = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (this.ShouldAlwaysReject) {
            return Task.FromResult(new InteractionLimiterResult { WasRejected = true });
        }

        string requestKey = key ?? $"{request.GetType().Name}:{request.Key}";
        int currentCount = this.counts.AddOrUpdate(requestKey, 1, (_, existingCount) => existingCount + 1);

        bool wasRejected = currentCount > this.MaxAllowedInteractions;

        return Task.FromResult(new InteractionLimiterResult { WasRejected = wasRejected });
    }

    /// <inheritdoc/>
    public Task ResetLimitAsync(IKeyedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string key = $"{request.GetType().Name}:{request.Key}";
        this.counts.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Helper method for tests to clear all stored counts between test runs.
    /// </summary>
    public void ResetAll()
    {
        this.counts.Clear();
        this.ShouldAlwaysReject = false;
    }
}
