namespace Dfe.SignIn.Base.Framework.UnitTests.Fakes;

public sealed class FakeServiceWithInteraction(IInteractionDispatcher interaction)
{
    public IInteractionDispatcher Interaction => interaction;
}
