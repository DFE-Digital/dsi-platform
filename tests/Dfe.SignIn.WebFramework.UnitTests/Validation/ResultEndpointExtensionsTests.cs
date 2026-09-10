using Dfe.SignIn.Base.Framework.Results;
using Dfe.SignIn.WebFramework.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.WebFramework.UnitTests.Validation;

[TestClass]
public sealed class ResultEndpointExtensionsTests
{
    [TestMethod]
    public void ToValidationProblem_SetsTypeFromErrorCode_WhenTypeNotSpecified()
    {
        var error = new Error("ChangeEmail.NoPendingRequest", "No pending request found", "VerificationCode");

        var result = error.ToValidationProblem();

        var problemResult = TypeAssert.IsType<ProblemHttpResult>(result);
        Assert.AreEqual("ChangeEmail.NoPendingRequest", problemResult.ProblemDetails.Type);
        Assert.AreEqual("No pending request found", problemResult.ProblemDetails.Detail);
    }

    [TestMethod]
    public void ToValidationProblem_UsesExplicitType_WhenSpecified()
    {
        var error = new Error("SomeCode", "Some detail");

        var result = error.ToValidationProblem(propertyName: "Field", type: "Custom.Type");

        var problemResult = TypeAssert.IsType<ProblemHttpResult>(result);
        Assert.AreEqual("Custom.Type", problemResult.ProblemDetails.Type);
    }

    [TestMethod]
    public void ToValidationProblem_FromFailureResult_SetsTypeAndField()
    {
        var error = new Error("ChangeEmail.InvalidCode", "Invalid code");
        var failedResult = Result.Failure(error);

        var result = failedResult.ToValidationProblem("VerificationCode");

        var problemResult = TypeAssert.IsType<ProblemHttpResult>(result);
        Assert.AreEqual("ChangeEmail.InvalidCode", problemResult.ProblemDetails.Type);
        Assert.AreEqual("Invalid code", problemResult.ProblemDetails.Detail);
    }

    [TestMethod]
    public void ToValidationProblem_ThrowsInvalidOperationException_WhenResultIsSuccess()
    {
        var successResult = Result.Success();

        Assert.ThrowsExactly<InvalidOperationException>(() => successResult.ToValidationProblem());
    }
}
