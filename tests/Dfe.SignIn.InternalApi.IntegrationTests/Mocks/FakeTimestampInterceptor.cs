using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

internal sealed class FakeTimestampInterceptor : TimestampInterceptor
{
    private readonly MutableTimeProvider mutableTimeProvider;

    public FakeTimestampInterceptor() : this(new MutableTimeProvider(TimeProvider.System)) { }

    private FakeTimestampInterceptor(MutableTimeProvider provider) : base(provider)
    {
        this.mutableTimeProvider = provider;
    }

    /// <summary>
    /// Gets or sets the underlying time provider. Swap at runtime without rebuilding DI.
    /// </summary>
    public TimeProvider TimeProvider {
        get => this.mutableTimeProvider.Inner;
        set => this.mutableTimeProvider.Inner = value;
    }

    public bool ShouldFail { get; set; }

    public bool ShouldSkipTimestamps { get; set; }

    /// <summary>
    /// Sets up the interceptor for a test. Can be called multiple times to change the state between tests.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for the interceptor.</param>
    /// <param name="shouldFail">Whether the interceptor should simulate a failure.</param>
    /// <param name="shouldSkipTimestamps">Whether the interceptor should skip timestamp updates.</param>
    /// <returns>The configured interceptor.</returns>
    public FakeTimestampInterceptor Setup(TimeProvider? timeProvider, bool shouldFail = false, bool shouldSkipTimestamps = false)
    {
        if (timeProvider != null) {
            this.TimeProvider = timeProvider;
        }

        this.ShouldFail = shouldFail;
        this.ShouldSkipTimestamps = shouldSkipTimestamps;

        return this;
    }

    /// <summary>
    /// Resets all test-specific state to defaults. Called between tests.
    /// </summary>
    public void Reset()
    {
        this.ShouldFail = false;
        this.ShouldSkipTimestamps = false;
        this.TimeProvider = TimeProvider.System;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (this.ShouldFail) {
            throw new DbUpdateException("Simulated database failure during save.", new Exception("Inner database exception constraint violation"));
        }

        if (this.ShouldSkipTimestamps) {
            return new ValueTask<InterceptionResult<int>>(result);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// A TimeProvider wrapper that delegates to a swappable inner provider.
    /// Registered once in DI; the inner provider can be changed per-test.
    /// </summary>
    private sealed class MutableTimeProvider(TimeProvider inner) : TimeProvider
    {
        public TimeProvider Inner { get; set; } = inner;
        public override DateTimeOffset GetUtcNow() => this.Inner.GetUtcNow();
    }
}
// Note: TimestampInterceptor is internal to Dfe.SignIn.Gateways.EntityFramework but is visible due to InternalsVisibleTo.
