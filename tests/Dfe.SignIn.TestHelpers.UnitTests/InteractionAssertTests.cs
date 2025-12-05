using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class InteractionAssertTests
{
    internal sealed record ExampleRequest
    {
    }

    private sealed class ExampleInteractorWithRequestValidation : IInteractor<ExampleRequest>
    {
        public Task<object> InvokeAsync(InteractionContext<ExampleRequest> context, CancellationToken cancellationToken)
        {
            context.ThrowIfHasValidationErrors();

            return Task.FromResult(new object());
        }
    }

    private sealed class ExampleInteractorWithoutRequestValidation : IInteractor<ExampleRequest>
    {
        public Task<object> InvokeAsync(InteractionContext<ExampleRequest> context, CancellationToken cancellationToken)
        {
            return Task.FromResult(new object());
        }
    }

    #region ThrowsWhenRequestIsInvalid<TRequest, TInteractor>()

    [TestMethod]
    public Task ThrowsWhenRequestIsInvalid_DoesNotThrow_WhenInteractorThrowsOnInvalidRequest()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<ExampleRequest, ExampleInteractorWithRequestValidation>();
    }

    [TestMethod]
    public Task ThrowsWhenRequestIsInvalid_Throws_WhenInteractorDoesNotThrowOnInvalidRequest()
    {
        return Assert.ThrowsExactlyAsync<AssertFailedException>(()
            => InteractionAssert.ThrowsWhenRequestIsInvalid<ExampleRequest, ExampleInteractorWithoutRequestValidation>());
    }

    #endregion

    #region HasValidationError(InvalidRequestException, string, string)

    [TestMethod]
    public void HasValidationError_DoesNotThrow_WhenExpectedValidationErrorIsPresent()
    {
        var exception = new InvalidRequestException(Guid.Empty, [
            new("Example error message.", ["OtherProperty", "ExampleProperty"])
        ]);

        InteractionAssert.HasValidationError(exception, "Example error message.", "ExampleProperty");
    }

    [TestMethod]
    public void HasValidationError_Throws_WhenExpectedValidationErrorIsNotPresent()
    {
        var exception = new InvalidRequestException();

        Assert.ThrowsExactly<AssertFailedException>(()
            => InteractionAssert.HasValidationError(exception, "Example error message.", "ExampleProperty"));
    }

    #endregion
}
