using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework.DataAnnotations;

namespace Dfe.SignIn.Base.Framework.UnitTests.DataAnnotations;

[TestClass]
public sealed class RequiredIfTargetEqualsAttributeTests
{
    internal sealed record ExampleModelWithMissingSourceProperty
    {
        [RequiredIfTargetEquals("MissingProperty", 42)]
        public string? ExampleProperty { get; set; }
    }

    internal sealed record ExampleModelWithNullableProperty
    {
        public int ExampleSourceProperty { get; set; }

        [RequiredIfTargetEquals(nameof(ExampleSourceProperty), 42, ErrorMessage = "Example error!")]
        public string? ExampleProperty { get; set; }
    }

    internal sealed record ExampleModelWithStringProperty
    {
        public int ExampleSourceProperty { get; set; }

        [RequiredIfTargetEquals(nameof(ExampleSourceProperty), 42, ErrorMessage = "Example error!")]
        public string? ExampleProperty { get; set; }
    }

    private static void ValidateExampleProperty(object model)
    {
        var context = new ValidationContext(model);

        var propertyInfo = model.GetType().GetProperty("ExampleProperty")!;

        context.MemberName = propertyInfo.Name;
        Validator.ValidateProperty(propertyInfo.GetValue(model), context);
    }

    #region IsValid(object?, ValidationContext)

    [TestMethod]
    public void IsValid_Throws_WhenSourcePropertyIsMissing()
    {
        var model = new ExampleModelWithMissingSourceProperty {
            ExampleProperty = null!,
        };

        var exception = Assert.ThrowsExactly<MissingMemberException>(()
            => ValidateExampleProperty(model));

        Assert.AreEqual("Unknown property: MissingProperty", exception.Message);
    }

    [TestMethod]
    public void IsValid_ThrowsValidationException_WhenRequiredAndNull()
    {
        var model = new ExampleModelWithNullableProperty {
            ExampleSourceProperty = 42,
            ExampleProperty = null,
        };

        var exception = Assert.Throws<ValidationException>(()
            => ValidateExampleProperty(model));

        Assert.AreEqual("Example error!", exception.ValidationResult.ErrorMessage);
        Assert.AreEqual("ExampleProperty", exception.ValidationResult.MemberNames.First());
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
    public void IsValid_ThrowsValidationException_WhenRequiredAndIsEmptyString(string value)
    {
        var model = new ExampleModelWithStringProperty {
            ExampleSourceProperty = 42,
            ExampleProperty = value,
        };

        var exception = Assert.Throws<ValidationException>(()
            => ValidateExampleProperty(model));

        Assert.AreEqual("Example error!", exception.ValidationResult.ErrorMessage);
        Assert.AreEqual("ExampleProperty", exception.ValidationResult.MemberNames.First());
    }

    [TestMethod]
    public void IsValid_DoesNotThrow_WhenNotRequired()
    {
        var model = new ExampleModelWithStringProperty {
            ExampleSourceProperty = 0,
            ExampleProperty = null,
        };

        ValidateExampleProperty(model);
    }

    [TestMethod]
    public void IsValid_DoesNotThrow_WhenRequiredAndValueIsPresent()
    {
        var model = new ExampleModelWithStringProperty {
            ExampleSourceProperty = 42,
            ExampleProperty = "Some value!",
        };
        var context = new ValidationContext(model);

        ValidateExampleProperty(model);
    }

    #endregion
}
