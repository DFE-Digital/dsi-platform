using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.NodeApiClient.UnitTests.Fakes;

public sealed record ExampleRequest
{
}

public sealed record ExampleResponse
{
}

[ApiRequester]
public sealed class ExampleApiRequesterUnspecifiedApi
    : IInteractor<ExampleRequest, ExampleResponse>
{
    public Task<ExampleResponse> InvokeAsync(ExampleRequest request) => throw new NotImplementedException();
}

[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class ExampleApiRequesterForAccessApi
    : IInteractor<ExampleRequest, ExampleResponse>
{
    public Task<ExampleResponse> InvokeAsync(ExampleRequest request) => throw new NotImplementedException();
}
