using System.Net;
using Dfe.SignIn.TestHelpers.Helpers;
using Dfe.SignIn.WebFramework.Mvc.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Validation;

[TestClass]
public sealed class ProblemDetailsMvcExtensionsTests
{
    [TestMethod]
    public async Task IsProblemType_ReturnsTrue_WhenTypeMatches()
    {
        var response = RefitTestHelper.CreateProblemResponse(
            HttpStatusCode.BadRequest,
            new ProblemDetails { Type = "ChangeEmail.NoPendingRequest" });

        var isMatch = await response.IsProblemType("ChangeEmail.NoPendingRequest");

        Assert.IsTrue(isMatch);
    }

    [TestMethod]
    public async Task IsProblemType_ReturnsFalse_WhenTypeDiffers()
    {
        var response = RefitTestHelper.CreateProblemResponse(
            HttpStatusCode.BadRequest,
            new ProblemDetails { Type = "ChangeEmail.InvalidCode" });

        var isMatch = await response.IsProblemType("ChangeEmail.NoPendingRequest");

        Assert.IsFalse(isMatch);
    }

    [TestMethod]
    public async Task IsProblemType_ReturnsFalse_WhenNoContent()
    {
        var response = RefitTestHelper.CreateProblemResponse(HttpStatusCode.InternalServerError);

        var isMatch = await response.IsProblemType("ChangeEmail.NoPendingRequest");

        Assert.IsFalse(isMatch);
    }

    [TestMethod]
    public async Task GetDetailAsync_ReturnsDetail_WhenProblemDetailsHasDetail()
    {
        var response = RefitTestHelper.CreateProblemResponse(
            HttpStatusCode.BadRequest,
            new ProblemDetails { Detail = "Some detail message" });

        var detail = await response.GetDetailAsync();

        Assert.AreEqual("Some detail message", detail);
    }

    [TestMethod]
    public async Task TryAddProblemDetailsToModelStateAsync_AddsValidationProblemErrorsToMappedField()
    {
        var modelState = new ModelStateDictionary();
        var validationProblem = new ValidationProblemDetails {
            Errors = {
                ["VerificationCode"] = ["Invalid code"]
            }
        };

        var response = RefitTestHelper.CreateProblemResponse(HttpStatusCode.BadRequest, validationProblem);
        var propertyMap = new Dictionary<string, string> {
            ["VerificationCode"] = "VerificationCodeInput"
        };

        await response.TryAddProblemDetailsToModelStateAsync(modelState, propertyMap);

        Assert.IsFalse(modelState.IsValid);
        var error = modelState["VerificationCodeInput"]?.Errors.Single();
        Assert.IsNotNull(error);
        Assert.AreEqual("Invalid code", error.ErrorMessage);
    }

    [TestMethod]
    public async Task TryAddProblemDetailsToModelStateAsync_AddsFallbackField_WhenGeneralProblemDetails()
    {
        var modelState = new ModelStateDictionary();
        var problem = new ProblemDetails { Detail = "General failure" };

        var response = RefitTestHelper.CreateProblemResponse(HttpStatusCode.BadRequest, problem);

        await response.TryAddProblemDetailsToModelStateAsync(modelState, fallbackField: "CustomField");

        Assert.IsFalse(modelState.IsValid);
        var error = modelState["CustomField"]?.Errors.Single();
        Assert.IsNotNull(error);
        Assert.AreEqual("General failure", error.ErrorMessage);
    }

    [TestMethod]
    public async Task TryAddProblemDetailsToModelStateAsync_AddsDefaultError_WhenNoContent()
    {
        var modelState = new ModelStateDictionary();
        var response = RefitTestHelper.CreateProblemResponse(HttpStatusCode.InternalServerError);

        await response.TryAddProblemDetailsToModelStateAsync(modelState, defaultErrorMessage: "Default error");

        Assert.IsFalse(modelState.IsValid);
        var error = modelState[string.Empty]?.Errors.Single();
        Assert.IsNotNull(error);
        Assert.AreEqual("Default error", error.ErrorMessage);
    }
}
