namespace Dfe.SignIn.Core.Framework.Tests.Fakes;

public sealed record AnotherExampleRequest
{
}

public sealed record AnotherExampleResponse
{
}

[InteractorContract]
public interface IAnotherExampleInteractor
    : IInteractor<AnotherExampleRequest, AnotherExampleResponse>;
