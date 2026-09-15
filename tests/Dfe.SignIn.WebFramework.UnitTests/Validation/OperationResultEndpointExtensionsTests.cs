using Dfe.SignIn.Base.Framework.OperationResults;
using Dfe.SignIn.WebFramework.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.WebFramework.UnitTests.Validation;

[TestClass]
public sealed class OperationResultEndpointExtensionsTests
{
    [TestMethod]
    public void ToValidationProblem_SetsTypeFromErrorCode_WhenTypeNotSpecified()
    {
        var error = new OperationError("ChangeEmail.NoPendingRequest", "No pending request found", "VerificationCode");

        var result = error.ToValidationProblem();

        var problemResult = TypeAssert.IsType<ProblemHttpResult>(result);
        Assert.AreEqual("ChangeEmail.NoPendingRequest", problemResult.ProblemDetails.Type);
        Assert.AreEqual("No pending request found", problemResult.ProblemDetails.Detail);
    }

    [TestMethod]
    public void ToValidationProblem_UsesExplicitType_WhenSpecified()
    {
        var error = new OperationError("SomeCode", "Some detail");

        var result = error.ToValidationProblem(propertyName: "Field", type: "Custom.Type");

        var problemResult = TypeAssert.IsType<ProblemHttpResult>(result);
        Assert.AreEqual("Custom.Type", problemResult.ProblemDetails.Type);
    }

    [TestMethod]
    public void ToValidationProblem_FromFailureResult_SetsTypeAndField()
    {
        var error = new OperationError("ChangeEmail.InvalidCode", "Invalid code");
        var failedResult = OperationResult.Failure(error);

        var result = failedResult.ToValidationProblem("VerificationCode");

        var problemResult = TypeAssert.IsType<ProblemHttpResult>(result);
        Assert.AreEqual("ChangeEmail.InvalidCode", problemResult.ProblemDetails.Type);
        Assert.AreEqual("Invalid code", problemResult.ProblemDetails.Detail);
    }

    [TestMethod]
    public void ToValidationProblem_ThrowsInvalidOperationException_WhenResultIsSuccess()
    {
        var successResult = OperationResult.Success();

        Assert.ThrowsExactly<InvalidOperationException>(() => successResult.ToValidationProblem());
    }
}
