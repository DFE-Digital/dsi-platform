using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;

namespace Dfe.SignIn.TestHelpers.Integration.Mocks;

public sealed class CapturingWriteToAuditInteractor : IInteractor<WriteToAuditRequest>
{
    public IReadOnlyList<WriteToAuditRequest> CapturedRequests => this.capturedRequests;
    private readonly List<WriteToAuditRequest> capturedRequests = [];

    public void Clear()
    {
        this.capturedRequests.Clear();
    }

    public Task<object> InvokeAsync(
        InteractionContext<WriteToAuditRequest> context,
        CancellationToken cancellationToken = default)
    {
        this.capturedRequests.Add(context.Request);
        return Task.FromResult<object>(new WriteToAuditResponse());
    }
}
