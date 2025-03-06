namespace Dfe.SignIn.Core.Framework.Tests.Fakes;

public sealed class FakeServiceWithInteraction(IInteractor<ExampleRequest, ExampleResponse> interaction)
{
    public IInteractor<ExampleRequest, ExampleResponse> Interaction => interaction;
}
