using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;

namespace Dfe.SignIn.TestHelpers.Integration.Mocks;

public sealed class TestAuditWriter : IAuditWriter
{
    private readonly CapturingWriteToAuditInteractor? interactorMock;

    public WriteToAuditRequest? CapturedRequest { get; private set; }

    public TestAuditWriter(CapturingWriteToAuditInteractor? interactorMock = null)
    {
        this.interactorMock = interactorMock;
    }

    public Task<WriteToAuditResponse> Log(InteractionContext<WriteToAuditRequest> context)
    {
        this.CapturedRequest = context.Request;
        if (this.interactorMock is not null) {
            this.interactorMock.InvokeAsync(context);
        }
        return Task.FromResult(new WriteToAuditResponse());
    }
}
