using Dfe.SignIn.Core.Framework;
using Moq.AutoMock;

namespace Dfe.SignIn.TestHelpers;

/// <summary>
/// Assertions for interaction framework.
/// </summary>
public static class InteractionAssert
{
    /// <summary>
    /// Asserts that interactor throws <see cref="InvalidRequestException"/> when context
    /// has at least one validation error.
    /// </summary>
    /// <typeparam name="TRequest">The type of interaction request.</typeparam>
    /// <typeparam name="TInteractor">The type of interactor that is under test.</typeparam>
    /// <param name="autoMocker">Automocker instance.</param>
    public static async Task ThrowsWhenRequestIsInvalid<TRequest, TInteractor>(
        AutoMocker? autoMocker = null)
        where TRequest : class
        where TInteractor : class, IInteractor<TRequest>
    {
        autoMocker ??= new AutoMocker();
        var useCase = autoMocker.CreateInstance<TInteractor>();

        var context = new InteractionContext<TRequest>(
            Activator.CreateInstance<TRequest>()
        );
        context.AddValidationError("Invalid request");

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => useCase.InvokeAsync(context));

        Assert.AreEqual(context.InvocationId, exception.InvocationId);
    }
}
