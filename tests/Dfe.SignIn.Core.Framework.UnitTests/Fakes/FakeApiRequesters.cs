namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

[ApiRequester]
public sealed class Example_ApiRequester
    : Interactor<ExampleRequest, ExampleResponse>
{
    public override Task<ExampleResponse> InvokeAsync(
        InteractionContext<ExampleRequest> context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new ExampleResponse { Name = "Test" }
        );
    }
}

[ApiRequester]
public sealed class AnotherExample_ApiRequester
    : Interactor<AnotherExampleRequest, AnotherExampleResponse>
{
    public override Task<AnotherExampleResponse> InvokeAsync(
        InteractionContext<AnotherExampleRequest> context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
