
namespace Dfe.SignIn.Core.Framework.Tests.Fakes;

[ApiRequester]
public sealed class ExampleApiRequester : IExampleInteractor
{
    public Task<ExampleResponse> HandleAsync(ExampleRequest request)
    {
        return Task.FromResult(
            new ExampleResponse { Name = "Test" }
        );
    }
}

[ApiRequester]
public sealed class AnotherExampleApiRequester : IAnotherExampleInteractor
{
    public Task<AnotherExampleResponse> HandleAsync(AnotherExampleRequest request)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
