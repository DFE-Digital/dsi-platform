namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class StringExtensionsTests
{
    [TestMethod]
    public void NormalizeWhitespace_Returns_EmptyStringWhenNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = StringExtensions.NormalizeWhitespace(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void NormalizeWhitespace_Returns_EmptyStringWhenEmpty()
    {
        var result = StringExtensions.NormalizeWhitespace(string.Empty);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    [DataRow("  Software Developer  ", "Software Developer")]
    [DataRow("Software Developer  ", "Software Developer")]
    [DataRow("  Software Developer", "Software Developer")]
    [DataRow("  Software      Developer   ", "Software Developer")]
    public void NormalizeWhitespace_Returns_EmptyStringWhenEmpty(string input, string expectedOutput)
    {
        var result = StringExtensions.NormalizeWhitespace(input);

        Assert.AreEqual(expectedOutput, result);
    }

    [TestMethod]
    [DataRow("58eb2690-5266-4cbb-ab46-1f4a211ce9c0")]
    [DataRow("58eb269052664cbbab461f4a211ce9c0")]
    [DataRow("{58eb2690-5266-4cbb-ab46-1f4a211ce9c0}")]
    [DataRow("(58eb2690-5266-4cbb-ab46-1f4a211ce9c0)")]
    public void ToGuid_ReturnsGuid_WhenValid(string input)
    {
        // Arrange
        var expected = new Guid(input);

        // Act
        var result = input.ToGuid();

        // Assert
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void ToGuid_ThrowsFormatException_WhenNull()
    {
        // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsExactly<FormatException>(() => StringExtensions.ToGuid(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("not-a-guid")]
    [DataRow("58eb2690-5266-4cbb-ab46-1f4a211ce9c")] // truncated
    public void ToGuid_ThrowsFormatException_WhenInvalid(string input)
    {
        // Act & Assert
        Assert.ThrowsExactly<FormatException>(() => input.ToGuid());
    }
}
