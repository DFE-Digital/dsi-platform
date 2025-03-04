
namespace Dfe.SignIn.Core.Framework.Tests.Fakes;

[UseCaseHandler]
public sealed class ExampleUseCaseHandler : IExampleInteractor
{
    public Task<ExampleResponse> InvokeAsync(ExampleRequest request)
    {
        return Task.FromResult(
            new ExampleResponse { Name = "Test" }
        );
    }
}

[UseCaseHandler]
public sealed class AnotherExampleUseCaseHandler : IAnotherExampleInteractor
{
    public Task<AnotherExampleResponse> InvokeAsync(AnotherExampleRequest request)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
