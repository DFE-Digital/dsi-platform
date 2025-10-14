namespace Dfe.SignIn.InternalApi.Client.UnitTests.Fakes;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> capturedRequests;
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc;
    public IReadOnlyList<HttpRequestMessage> CapturedRequests => this.capturedRequests.AsReadOnly();

    public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
    {
        this.handlerFunc = handlerFunc;
        this.capturedRequests = [];
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        this.capturedRequests.Add(request);
        return this.handlerFunc(request, cancellationToken);
    }
}
