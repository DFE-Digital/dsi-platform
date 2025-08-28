namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

public sealed class FakeServiceWithInteraction(IInteractionDispatcher interaction)
{
    public IInteractionDispatcher Interaction => interaction;
}
