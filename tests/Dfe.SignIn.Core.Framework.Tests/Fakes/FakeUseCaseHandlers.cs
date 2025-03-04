
namespace Dfe.SignIn.Core.Framework.Tests.Fakes;

[UseCaseHandler]
public sealed class ExampleUseCaseHandler : IExampleInteractor
{
    public Task<ExampleResponse> HandleAsync(ExampleRequest request)
    {
        return Task.FromResult(
            new ExampleResponse { Name = "Test" }
        );
    }
}

[UseCaseHandler]
public sealed class AnotherExampleUseCaseHandler : IAnotherExampleInteractor
{
    public Task<AnotherExampleResponse> HandleAsync(AnotherExampleRequest request)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
