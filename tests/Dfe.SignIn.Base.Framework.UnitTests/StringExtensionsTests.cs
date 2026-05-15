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
}
