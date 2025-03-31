namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

[UseCaseHandler]
public sealed class Example_UseCaseHandler
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

[UseCaseHandler]
public sealed class AnotherExample_UseCaseHandler
    : IInteractor<AnotherExampleRequest, AnotherExampleResponse>
{
    public Task<AnotherExampleResponse> InvokeAsync(
        AnotherExampleRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
