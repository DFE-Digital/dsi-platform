namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

public sealed class FakeServiceWithInteraction(IInteractor<ExampleRequest, ExampleResponse> interaction)
{
    public IInteractor<ExampleRequest, ExampleResponse> Interaction => interaction;
}
