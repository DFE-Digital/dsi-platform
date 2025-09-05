namespace Dfe.SignIn.Base.Framework.UnitTests.Fakes;

public sealed record ExampleRequest
{
}

public sealed record ExampleResponse
{
    public required string Name { get; init; }
}
