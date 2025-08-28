using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Framework.UnitTests.Fakes;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class InteractorValidationTests
{
    #region TryValidateRequest(object, ICollection<ValidationResult>)

    [TestMethod]
    public void TryValidateRequest_Throws_WhenRequestModelArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var exception = Assert.Throws<ArgumentNullException>(() => {
            var validationResults = new List<ValidationResult>();
            validator.TryValidateRequest(null!, validationResults);
        });
    }

    [TestMethod]
    public void TryValidateRequest_Throws_WhenValidationResultsArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var exception = Assert.Throws<ArgumentNullException>(() => {
            var model = new object();
            validator.TryValidateRequest(model, null!);
        });
    }

    [TestMethod]
    public void TryValidateRequest_ReturnsTrue_WhenModelIsValid()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var model = new ExampleInteractorWithValidationRequest {
            Name = "Example",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        };

        var validationResults = new List<ValidationResult>();
        bool result = validator.TryValidateRequest(model, validationResults);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TryValidateRequest_NoValidationResults_WhenModelIsValid()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var model = new ExampleInteractorWithValidationRequest {
            Name = "Example",
            SomeEnumProperty = ExampleInteractorEnum.FirstValue,
        };

        var validationResults = new List<ValidationResult>();
        validator.TryValidateRequest(model, validationResults);

        Assert.IsEmpty(validationResults);
    }

    [TestMethod]
    public void TryValidateRequest_ReturnsFalse_WhenModelIsNotValid()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var model = new ExampleInteractorWithValidationRequest {
            Name = "E",
            SomeEnumProperty = (ExampleInteractorEnum)(-1),
        };

        var validationResults = new List<ValidationResult>();
        bool result = validator.TryValidateRequest(model, validationResults);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryValidateRequest_MultipleValidationResults_WhenModelIsNotValid()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var model = new ExampleInteractorWithValidationRequest {
            Name = "E",
            SomeEnumProperty = (ExampleInteractorEnum)(-1),
        };

        var validationResults = new List<ValidationResult>();
        validator.TryValidateRequest(model, validationResults);

        Assert.HasCount(2, validationResults);
        Assert.IsTrue(validationResults.Any(x => x.MemberNames.First() == nameof(ExampleInteractorWithValidationRequest.Name)));
        Assert.IsTrue(validationResults.Any(x => x.MemberNames.First() == nameof(ExampleInteractorWithValidationRequest.SomeEnumProperty)));
    }

    #endregion

    #region TryValidateResponse(object, ICollection<ValidationResult>)

    [TestMethod]
    public void TryValidateResponse_Throws_WhenRequestModelArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var exception = Assert.Throws<ArgumentNullException>(() => {
            var validationResults = new List<ValidationResult>();
            validator.TryValidateResponse(null!, validationResults);
        });
    }

    [TestMethod]
    public void TryValidateResponse_Throws_WhenValidationResultsArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var exception = Assert.Throws<ArgumentNullException>(() => {
            var model = new object();
            validator.TryValidateResponse(model, null!);
        });
    }

    [TestMethod]
    public void TryValidateResponse_ReturnsTrue_WhenModelIsValid()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var model = new ExampleInteractorWithValidationResponse {
            Percentage = 0.5f,
        };

        var validationResults = new List<ValidationResult>();
        bool result = validator.TryValidateResponse(model, validationResults);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TryValidateResponse_NoValidationResults_WhenModelIsValid()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var model = new ExampleInteractorWithValidationResponse {
            Percentage = 0.5f,
        };

        var validationResults = new List<ValidationResult>();
        validator.TryValidateResponse(model, validationResults);

        Assert.IsEmpty(validationResults);
    }

    [TestMethod]
    public void TryValidateResponse_ReturnsFalse_WhenModelIsNotValid()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var model = new ExampleInteractorWithValidationResponse {
            Percentage = 2.5f,
        };

        var validationResults = new List<ValidationResult>();
        bool result = validator.TryValidateResponse(model, validationResults);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryValidateResponse_MultipleValidationResults_WhenModelIsNotValid()
    {
        var autoMocker = new AutoMocker();
        var validator = autoMocker.CreateInstance<InteractionValidator>();

        var model = new ExampleInteractorWithValidationResponse {
            Percentage = 2.5f,
        };

        var validationResults = new List<ValidationResult>();
        validator.TryValidateResponse(model, validationResults);

        Assert.HasCount(1, validationResults);
        Assert.IsTrue(validationResults.Any(x => x.MemberNames.First() == nameof(ExampleInteractorWithValidationResponse.Percentage)));
    }

    #endregion
}
