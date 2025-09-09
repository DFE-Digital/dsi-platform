using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

public sealed record ExampleRequest
{
}

public sealed record ExampleResponse
{
}

[ApiRequester]
public sealed class ExampleApiRequesterUnspecifiedApi
    : Interactor<ExampleRequest, ExampleResponse>
{
    public override Task<ExampleResponse> InvokeAsync(
        InteractionContext<ExampleRequest> context,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class ExampleApiRequesterForAccessApi
    : Interactor<ExampleRequest, ExampleResponse>
{
    public override Task<ExampleResponse> InvokeAsync(
        InteractionContext<ExampleRequest> context,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

[ApiRequester]
[NodeApi(NodeApiName.Access)]
[NodeApi(NodeApiName.Directories)]
public sealed class ExampleApiRequesterForAccessAndDirectoriesApi
    : Interactor<ExampleRequest, ExampleResponse>
{
    public override Task<ExampleResponse> InvokeAsync(
        InteractionContext<ExampleRequest> context,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
