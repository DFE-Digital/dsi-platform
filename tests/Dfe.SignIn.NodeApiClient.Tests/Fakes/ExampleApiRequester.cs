using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.NodeApiClient.Tests.Fakes;

public sealed record ExampleRequest
{
}

public sealed record ExampleResponse
{
}

public sealed class ExampleApiRequesterUnspecifiedApi
    : IApiRequester<ExampleRequest, ExampleResponse>
{
    public Task<ExampleResponse> HandleAsync(ExampleRequest request) => throw new NotImplementedException();
}

[NodeApi(NodeApiName.Access)]
public sealed class ExampleApiRequesterForAccessApi
    : IApiRequester<ExampleRequest, ExampleResponse>
{
    public Task<ExampleResponse> HandleAsync(ExampleRequest request) => throw new NotImplementedException();
}
