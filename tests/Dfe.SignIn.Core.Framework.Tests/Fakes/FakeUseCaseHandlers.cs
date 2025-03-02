
namespace Dfe.SignIn.Core.Framework.Tests.Fakes;

public sealed class ExampleUseCaseHandler
    : IUseCaseHandler<ExampleRequest, ExampleResponse>
{
    public Task<ExampleResponse> HandleAsync(ExampleRequest request)
    {
        return Task.FromResult(
            new ExampleResponse { Name = "Test" }
        );
    }
}

public sealed class AnotherExampleUseCaseHandler
    : IUseCaseHandler<AnotherExampleRequest, AnotherExampleResponse>
{
    public Task<AnotherExampleResponse> HandleAsync(AnotherExampleRequest request)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
