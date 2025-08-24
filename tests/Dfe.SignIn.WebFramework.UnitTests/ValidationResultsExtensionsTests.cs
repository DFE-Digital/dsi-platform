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

        void Act()
        {
            ValidationResultsExtensions.AddFrom(null!, validationResults, []);
        }

        Assert.Throws<ArgumentNullException>(Act);
    }

    [TestMethod]
    public void AddFrom_Throws_WhenValidationResultsArgumentIsNull()
    {
        var modelState = new ModelStateDictionary();

        void Act()
        {
            ValidationResultsExtensions.AddFrom(modelState, null!, []);
        }

        Assert.Throws<ArgumentNullException>(Act);
    }

    [TestMethod]
    public void AddFrom_Throws_WhenMemberNameMappingsArgumentIsNull()
    {
        var modelState = new ModelStateDictionary();
        var validationResults = Array.Empty<ValidationResult>();

        void Act()
        {
            ValidationResultsExtensions.AddFrom(modelState, validationResults, null!);
        }

        Assert.Throws<ArgumentNullException>(Act);
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
}
