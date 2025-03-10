namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

public sealed record ExampleRequest
{
}

public sealed record ExampleResponse
{
    public required string Name { get; init; }
}
