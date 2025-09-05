using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework.UnitTests.Fakes;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class InteractionContextTests
{
    #region AddValidationError(string?, string?)

    [TestMethod]
    public void AddValidationError_AddsEmptyError()
    {
        var context = new InteractionContext<ExampleRequest>(new ExampleRequest());

        context.AddValidationError(null);

        Assert.HasCount(1, context.ValidationResults);
    }

    [TestMethod]
    public void AddValidationError_AddsErrorMessage()
    {
        var context = new InteractionContext<ExampleRequest>(new ExampleRequest());

        context.AddValidationError("Example error message");

        Assert.HasCount(1, context.ValidationResults);
        Assert.AreEqual("Example error message", context.ValidationResults.First().ErrorMessage);
    }

    [TestMethod]
    public void AddValidationError_AddsErrorMessageWithPropertyName()
    {
        var context = new InteractionContext<ExampleRequest>(new ExampleRequest());

        context.AddValidationError("Example error message", "FakePropertyName");

        Assert.HasCount(1, context.ValidationResults);
        Assert.AreEqual("Example error message", context.ValidationResults.First().ErrorMessage);
        Assert.Contains("FakePropertyName", context.ValidationResults.First().MemberNames);
    }

    #endregion

    #region ThrowIfHasValidationErrors()

    [TestMethod]
    public void ThrowIfHasValidationErrors_DoesNotThrow_WhenThereAreNoValidationErrors()
    {
        var context = new InteractionContext<ExampleRequest>(new ExampleRequest());

        try {
            context.ThrowIfHasValidationErrors();
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    [TestMethod]
    public void ThrowIfHasValidationErrors_DoesThrow_WhenThereAreValidationErrors()
    {
        var context = new InteractionContext<ExampleRequest>(new ExampleRequest());
        context.ValidationResults.Add(new ValidationResult("An example error."));

        var exception = Assert.ThrowsExactly<InvalidRequestException>(context.ThrowIfHasValidationErrors);

        Assert.HasCount(1, exception.ValidationResults);
    }

    #endregion

    #region implicit operator InteractionContext<TRequest>(TRequest)

    [TestMethod]
    public void ImplicitOperator_InteractionContext_ReturnsInteractionContextWithRequest()
    {
        var fakeRequest = new ExampleRequest();

        InteractionContext<ExampleRequest> context = fakeRequest;

        Assert.AreSame(fakeRequest, context.Request);
    }

    #endregion
}
