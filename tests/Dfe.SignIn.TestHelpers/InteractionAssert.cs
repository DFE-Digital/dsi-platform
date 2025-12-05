using Dfe.SignIn.Base.Framework;
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

    /// <summary>
    /// Asserts that the expected validation error has occurred.
    /// </summary>
    /// <param name="exception">The invalid request exception.</param>
    /// <param name="expectedMessage">The expected error message.</param>
    /// <param name="memberName">Name of the request property.</param>
    public static void HasValidationError(InvalidRequestException exception, string expectedMessage, string memberName)
    {
        var result = exception.ValidationResults?.FirstOrDefault(
            result => result.MemberNames.Contains(memberName)
        );

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedMessage, result.ErrorMessage);
    }
}
