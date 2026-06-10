using Dfe.SignIn.Base.Framework.Internal;

namespace Dfe.SignIn.Base.Framework.UnitTests.Internal;

[TestClass]
public sealed class EnumHelpersTests
{
    public enum EnumTest
    {
        Unknown = 0,
        [System.ComponentModel.Description("First value description")]
        One = 1,
        [System.ComponentModel.Description("Second value description")]
        Two = 2,
        Three = 3
    }

    [TestMethod]
    [DataRow("One", EnumTest.One)]
    [DataRow("two", EnumTest.Two)]
    [DataRow("3", EnumTest.Three)]
    [DataRow(1, EnumTest.One)]
    [DataRow(2, EnumTest.Two)]
    public void MapEnum_ValidInput_ReturnsEnum(object input, EnumTest expected)
    {
        var result = EnumHelpers.MapEnum<EnumTest>(input);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("InvalidValue")]
    [DataRow(99)]
    [DataRow(3.14)]
    public void MapEnum_InvalidInput_Throws(object input)
    {
        if (input == null) {
            Assert.ThrowsExactly<ArgumentNullException>(
            () => EnumHelpers.MapEnum<EnumTest>(input));
        }
        else {
            Assert.ThrowsExactly<ArgumentException>(
            () => EnumHelpers.MapEnum<EnumTest>(input));
        }
    }

    [TestMethod]
    [DataRow(EnumTest.One, "First value description")]
    [DataRow(EnumTest.Two, "Second value description")]
    public void GetDescription_WithDescriptionAttribute_ReturnsDescription(
      EnumTest value,
      string expected)
    {
        var result = value.GetDescription();
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void GetDescription_WithoutDescriptionAttribute_ReturnsEnumName()
    {
        var result = EnumTest.Three.GetDescription();
        Assert.AreEqual("Three", result);
    }
}
