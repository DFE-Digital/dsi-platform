using Dfe.SignIn.Core.Public.Metadata;

namespace Dfe.SignIn.Core.Public.UnitTests.Metadata;

[TestClass]
public sealed class AnnotationHelpersTests
{
    #region GetDescription(Enum)

    private enum FakeEnumForDescriptionTests
    {
        [System.ComponentModel.Description("An example description.")]
        ExampleWithDescription,

        ExampleWithoutDescription,
    }

    [TestMethod]
    public void GetDescription_ReturnsDescription()
    {
        var enumValue = FakeEnumForDescriptionTests.ExampleWithDescription;

        string? description = AnnotationHelpers.GetDescription(enumValue);

        Assert.AreEqual("An example description.", description);
    }

    [TestMethod]
    public void GetDescription_ReturnsNull_WhenNotAnnotatedWithDescription()
    {
        var enumValue = FakeEnumForDescriptionTests.ExampleWithoutDescription;

        string? description = AnnotationHelpers.GetDescription(enumValue);

        Assert.IsNull(description);
    }

    #endregion

    #region GetTagColour(Enum)

    private enum FakeEnumForTagColourTests
    {
        [TagColour(TagColour.Blue)]
        ExampleWithTagColour,

        ExampleWithoutTagColour,
    }

    [TestMethod]
    public void GetTagColour_ReturnsTagColour()
    {
        var enumValue = FakeEnumForTagColourTests.ExampleWithTagColour;

        TagColour? tagColour = AnnotationHelpers.GetTagColour(enumValue);

        Assert.AreEqual(TagColour.Blue, tagColour);
    }

    [TestMethod]
    public void GetTagColour_ReturnsNull_WhenNotAnnotatedWithTagColour()
    {
        var enumValue = FakeEnumForTagColourTests.ExampleWithoutTagColour;

        TagColour? tagColour = AnnotationHelpers.GetTagColour(enumValue);

        Assert.IsNull(tagColour);
    }

    #endregion
}
