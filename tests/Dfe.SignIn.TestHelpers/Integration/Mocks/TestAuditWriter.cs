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

    public Task<WriteToAuditResponse> Log(WriteToAuditRequest request)
    {
        this.CapturedRequest = request;
        this.interactorMock?.InvokeAsync(request);
        return Task.FromResult(new WriteToAuditResponse());
    }
}
