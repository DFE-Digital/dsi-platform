namespace Dfe.SignIn.Core.Framework.Tests.Fakes;

public sealed record ExampleRequest
{
}

public sealed record ExampleResponse
{
    public required string Name { get; init; }
}
