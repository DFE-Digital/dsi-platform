namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

public sealed class FakeInteractionException(string? message = null, Exception? innerException = null)
    : InteractionException(message, innerException);
