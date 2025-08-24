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
    public static async Task ThrowsWhenRequestIsInvalid<TRequest, TInteractor>()
        where TRequest : class
        where TInteractor : class, IInteractor<TRequest>
    {
        var autoMocker = new AutoMocker();
        var useCase = autoMocker.CreateInstance<TInteractor>();

        var context = new InteractionContext<TRequest>(
            Activator.CreateInstance<TRequest>()
        );
        context.AddValidationError("Invalid request");

        Task Act()
        {
            return useCase.InvokeAsync(context);
        }

        var exception = await Assert.ThrowsAsync<InvalidRequestException>(Act);
        Assert.AreEqual(context.InvocationId, exception.InvocationId);
    }
}
