using Dfe.SignIn.Core.Framework.Internal;

namespace Dfe.SignIn.Core.Framework.UnitTests.Internal;

[TestClass]
public sealed class EnumHelpersTests
{
    public enum EnumTest
    {
        Unknown = 0,
        One = 1,
        Two = 2,
        Three = 3
    }

    [DataTestMethod]
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

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("InvalidValue")]
    [DataRow(99)]
    [DataRow(3.14)]
    public void MapEnum_InvalidInput_Throws(object input)
    {
        Type exceptionType;

        if (input == null) {
            exceptionType = typeof(ArgumentNullException);
        }
        else {
            exceptionType = typeof(ArgumentException);
        }

        try {
            EnumHelpers.MapEnum<EnumTest>(input);
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (Exception ex) {
            Assert.IsInstanceOfType(ex, exceptionType);
        }
    }
}
