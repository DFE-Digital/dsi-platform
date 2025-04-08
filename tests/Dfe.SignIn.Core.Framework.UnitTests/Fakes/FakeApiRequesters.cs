namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

[ApiRequester]
public sealed class Example_ApiRequester
    : IInteractor<ExampleRequest, ExampleResponse>
{
    public Task<ExampleResponse> InvokeAsync(
        ExampleRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new ExampleResponse { Name = "Test" }
        );
    }
}

[ApiRequester]
public sealed class AnotherExample_ApiRequester
    : IInteractor<AnotherExampleRequest, AnotherExampleResponse>
{
    public Task<AnotherExampleResponse> InvokeAsync(
        AnotherExampleRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
