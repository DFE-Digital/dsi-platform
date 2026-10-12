using Dfe.SignIn.Base.Framework.OperationResults;
using Dfe.SignIn.WebFramework.Mvc.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Validation;

[TestClass]
public sealed class OperationResultsMvcExtensionsTests
{
    [TestMethod]
    public void AddToModelState_AddsErrorToTarget_WhenOperationFailed()
    {
        var modelState = new ModelStateDictionary();
        var result = OperationResult.Failure(new OperationError("Invalid", "Name is invalid.", "FirstName"));

        result.AddToModelState(modelState);

        Assert.AreEqual("Name is invalid.", modelState["FirstName"]!.Errors[0].ErrorMessage);
    }

    [TestMethod]
    public void AddToModelState_AddsModelLevelError_WhenFailureHasNoTarget()
    {
        var modelState = new ModelStateDictionary();
        var result = OperationResult.Failure(new OperationError("Invalid", "Request is invalid."));

        result.AddToModelState(modelState);

        Assert.AreEqual("Request is invalid.", modelState[string.Empty]!.Errors[0].ErrorMessage);
    }

    [TestMethod]
    public void AddToModelState_DoesNotAddError_WhenOperationSucceeded()
    {
        var modelState = new ModelStateDictionary();

        OperationResult.Success().AddToModelState(modelState);

        Assert.IsTrue(modelState.IsValid);
    }
}
