using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

internal sealed class TestTimestampInterceptor(TimeProvider timeProvider) : TimestampInterceptor(timeProvider)
{
    public bool ShouldFail { get; set; }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (this.ShouldFail) {
            throw new DbUpdateException("Simulated database failure during save.", new Exception("Inner database exception constraint violation"));
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
// Note: TimestampInterceptor is internal to Dfe.SignIn.Gateways.EntityFramework but is visible due to InternalsVisibleTo.
