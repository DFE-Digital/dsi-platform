using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

internal sealed class FailingDbCommandInterceptor : DbCommandInterceptor
{
    public bool IsEnabled { get; set; }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (this.IsEnabled) {
            throw new InvalidOperationException("Simulated database failure during ExecuteDeleteAsync.");
        }

        return new ValueTask<InterceptionResult<int>>(result);
    }

    public void Reset() => this.IsEnabled = false;
}
