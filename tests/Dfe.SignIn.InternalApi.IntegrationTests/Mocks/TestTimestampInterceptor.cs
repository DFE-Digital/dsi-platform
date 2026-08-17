using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

internal sealed class TestTimestampInterceptor : TimestampInterceptor
{
    private readonly MutableTimeProvider mutableTimeProvider;

    public TestTimestampInterceptor() : this(new MutableTimeProvider(TimeProvider.System)) { }

    private TestTimestampInterceptor(MutableTimeProvider provider) : base(provider)
    {
        this.mutableTimeProvider = provider;
    }

    /// <summary>
    /// Gets or sets the underlying time provider. Swap at runtime without rebuilding DI.
    /// </summary>
    public TimeProvider TimeProvider
    {
        get => this.mutableTimeProvider.Inner;
        set => this.mutableTimeProvider.Inner = value;
    }

    public bool ShouldFail { get; set; }

    public bool ShouldSkipTimestamps { get; set; }

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
