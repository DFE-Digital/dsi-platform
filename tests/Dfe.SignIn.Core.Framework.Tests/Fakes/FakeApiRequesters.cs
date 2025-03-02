
namespace Dfe.SignIn.Core.Framework.Tests.Fakes;

public sealed class ExampleApiRequester : IApiRequester<ExampleRequest, ExampleResponse>
{
    public Task<ExampleResponse> HandleAsync(ExampleRequest request)
    {
        return Task.FromResult(
            new ExampleResponse { Name = "Test" }
        );
    }
}

public sealed class AnotherExampleApiRequester : IApiRequester<AnotherExampleRequest, AnotherExampleResponse>
{
    public Task<AnotherExampleResponse> HandleAsync(AnotherExampleRequest request)
    {
        return Task.FromResult(new AnotherExampleResponse());
    }
}
