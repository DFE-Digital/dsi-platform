using Dfe.SignIn.Core.Framework;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.WebFramework.UnitTests;

[TestClass]
public sealed class ValidationResultsExtensionsTests
{
    private sealed record ExampleRequest
    {
        public required int ModelProperty { get; init; }
    }

    private sealed class ExampleViewModel
    {
        public int ViewModelProperty { get; set; }
    }

    #region AddFrom(ModelStateDictionary, IEnumerable<ValidationResult>, Dictionary<string, string>

    [TestMethod]
    public void AddFrom_Throws_WhenModelStateArgumentIsNull()
    {
        var validationResults = Array.Empty<ValidationResult>();

        Assert.Throws<ArgumentNullException>(()
            => ValidationResultsExtensions.AddFrom(null!, validationResults, []));
    }

    [TestMethod]
    public void AddFrom_Throws_WhenValidationResultsArgumentIsNull()
    {
        var modelState = new ModelStateDictionary();

        Assert.Throws<ArgumentNullException>(()
            => ValidationResultsExtensions.AddFrom(modelState, null!, []));
    }

    [TestMethod]
    public void AddFrom_Throws_WhenMemberNameMappingsArgumentIsNull()
    {
        var modelState = new ModelStateDictionary();
        var validationResults = Array.Empty<ValidationResult>();

        Assert.Throws<ArgumentNullException>(()
            => ValidationResultsExtensions.AddFrom(modelState, validationResults, null!));
    }

    [TestMethod]
    public void AddFrom_AddsValidationResult_WhenMemberNameIsMappedToViewModel()
    {
        var modelState = new ModelStateDictionary();
        var validationResults = new ValidationResult[] {
            new("Example validation error", [nameof(ExampleRequest.ModelProperty)])
        };

        ValidationResultsExtensions.AddFrom(modelState, validationResults, new() {
            [nameof(ExampleRequest.ModelProperty)] = nameof(ExampleViewModel.ViewModelProperty),
        });

        Assert.AreEqual(1, modelState.ErrorCount);

        var error = modelState[nameof(ExampleViewModel.ViewModelProperty)]?.Errors.FirstOrDefault();
        Assert.IsNotNull(error);
        Assert.AreEqual("Example validation error", error.ErrorMessage);
    }

    [TestMethod]
    public void AddFrom_IgnoresValidationResult_WhenMemberNameIsNotMappedToViewModel()
    {
        var modelState = new ModelStateDictionary();
        var validationResults = new ValidationResult[] {
            new("Example validation error", [nameof(ExampleRequest.ModelProperty)])
        };

        ValidationResultsExtensions.AddFrom(modelState, validationResults, []);

        Assert.AreEqual(0, modelState.ErrorCount);
    }

    #endregion

    #region ThrowIfNoErrorsRecorded(ModelStateDictionary, Exception?)

    [TestMethod]
    public void ThrowIfNoErrorsRecorded_Throws_WhenModelStateArgumentIsNull()
    {
        var validationResults = Array.Empty<ValidationResult>();

        Assert.Throws<ArgumentNullException>(()
            => ValidationResultsExtensions.ThrowIfNoErrorsRecorded(null!));
    }

    [TestMethod]
    public void ThrowIfNoErrorsRecorded_ThrowsGivenException_WhenModelStateIsValid()
    {
        var validationResults = Array.Empty<ValidationResult>();

        var modelState = new ModelStateDictionary();
        var expectedException = new InvalidRequestException();

        var actualException = Assert.Throws<InvalidRequestException>(()
            => ValidationResultsExtensions.ThrowIfNoErrorsRecorded(modelState, expectedException));

        Assert.AreSame(expectedException, actualException);
    }

    [TestMethod]
    public void ThrowIfNoErrorsRecorded_ThrowsInvalidOperation_WhenModelStateIsValid()
    {
        var validationResults = Array.Empty<ValidationResult>();

        var modelState = new ModelStateDictionary();

        Assert.Throws<InvalidOperationException>(()
            => ValidationResultsExtensions.ThrowIfNoErrorsRecorded(modelState));
    }

    [TestMethod]
    public void ThrowIfNoErrorsRecorded_DoesNotThrow_WhenModelStateIsInvalid()
    {
        var validationResults = Array.Empty<ValidationResult>();

        var modelState = new ModelStateDictionary();
        modelState.AddModelError("EmailAddress", "Enter a valid email address.");

        ValidationResultsExtensions.ThrowIfNoErrorsRecorded(modelState);
    }

    #endregion
}
