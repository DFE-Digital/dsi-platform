namespace Dfe.SignIn.Base.Framework.UnitTests.Fakes;

public sealed class ExampleUseCase
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

public sealed class AnotherExampleUseCase
    : Interactor<AnotherExampleRequest, AnotherExampleResponse>
{
    public override Task<AnotherExampleResponse> InvokeAsync(
        InteractionContext<AnotherExampleRequest> request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
