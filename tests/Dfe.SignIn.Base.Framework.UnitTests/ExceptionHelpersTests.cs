namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class ExceptionHelpersTests
{
    #region ThrowIfArgumentNull(object, string)

    [TestMethod]
    public void ThrowIfArgumentNull_Throws_WhenValueIsNull()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => ExceptionHelpers.ThrowIfArgumentNull(null!, "exampleParam")
        );
        Assert.AreEqual("exampleParam", exception.ParamName);
    }

    [TestMethod]
    public void ThrowIfArgumentNull_DoesNotThrow_WhenValueIsValid()
    {
        try {
            ExceptionHelpers.ThrowIfArgumentNull("Not null!", "exampleParam");
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion

    #region ThrowIfArgumentNullOrEmpty(object, string)

    [TestMethod]
    public void ThrowIfArgumentNullOrEmpty_Throws_WhenValueIsNull()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => ExceptionHelpers.ThrowIfArgumentNullOrEmpty(null!, "exampleParam")
        );
        Assert.AreEqual("exampleParam", exception.ParamName);
    }

    [TestMethod]
    public void ThrowIfArgumentNullOrEmpty_Throws_WhenValueIsEmptyString()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(
            () => ExceptionHelpers.ThrowIfArgumentNullOrEmpty("", "exampleParam")
        );
        Assert.AreEqual("exampleParam", exception.ParamName);
    }

    [TestMethod]
    public void ThrowIfArgumentNullOrEmpty_DoesNotThrow_WhenValueIsValid()
    {
        try {
            ExceptionHelpers.ThrowIfArgumentNullOrEmpty("Not null or empty!", "exampleParam");
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion

    #region ThrowIfArgumentNullOrWhiteSpace(object, string)

    [TestMethod]
    public void ThrowIfArgumentNullOrWhiteSpace_Throws_WhenValueIsNull()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(null!, "exampleParam")
        );
        Assert.AreEqual("exampleParam", exception.ParamName);
    }

    [DataRow("   ")]
    [DataRow("")]
    [TestMethod]
    public void ThrowIfArgumentNullOrWhiteSpace_Throws_WhenValueIsWhiteSpace(string value)
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(
            () => ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(value, "exampleParam")
        );
        Assert.AreEqual("exampleParam", exception.ParamName);
    }

    [TestMethod]
    public void ThrowIfArgumentNullOrWhiteSpace_DoesNotThrow_WhenValueIsValid()
    {
        try {
            ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace("Not null or just white space!", "exampleParam");
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion
}
