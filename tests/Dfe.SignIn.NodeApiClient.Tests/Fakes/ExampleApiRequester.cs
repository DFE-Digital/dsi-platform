using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.NodeApiClient.Tests.Fakes;

public sealed record ExampleRequest
{
}

public sealed record ExampleResponse
{
}

[InteractorContract]
public interface IExampleInteractor : IInteractor<ExampleRequest, ExampleResponse>;

[ApiRequester]
public sealed class ExampleApiRequesterUnspecifiedApi : IExampleInteractor
{
    public Task<ExampleResponse> HandleAsync(ExampleRequest request) => throw new NotImplementedException();
}

[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class ExampleApiRequesterForAccessApi : IExampleInteractor
{
    public Task<ExampleResponse> HandleAsync(ExampleRequest request) => throw new NotImplementedException();
}
