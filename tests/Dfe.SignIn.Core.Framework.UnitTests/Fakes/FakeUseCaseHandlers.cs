namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

[UseCaseHandler]
public sealed class Example_UseCaseHandler
    : Interactor<ExampleRequest, ExampleResponse>
{
    public override Task<ExampleResponse> InvokeAsync(
        InteractionContext<ExampleRequest> request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            new ExampleResponse { Name = "Test" }
        );
    }
}

[UseCaseHandler]
public sealed class AnotherExample_UseCaseHandler
    : Interactor<AnotherExampleRequest, AnotherExampleResponse>
{
    public override Task<AnotherExampleResponse> InvokeAsync(
        InteractionContext<AnotherExampleRequest> request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
