using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;

namespace Dfe.SignIn.TestHelpers.Integration.Mocks;

public sealed class CapturingWriteToAuditInteractor : IInteractor<WriteToAuditRequest>
{
    public WriteToAuditRequest? CapturedRequest { get; private set; }
    public IReadOnlyList<WriteToAuditRequest> CapturedRequests => this.capturedRequests;
    private readonly List<WriteToAuditRequest> capturedRequests = [];

    public Task<object> InvokeAsync(
        InteractionContext<WriteToAuditRequest> context,
        CancellationToken cancellationToken = default)
    {
        this.CapturedRequest = context.Request;
        this.capturedRequests.Add(context.Request);
        return Task.FromResult<object>(new WriteToAuditResponse());
    }
}
