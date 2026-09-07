namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class ResultTests
{
    [TestMethod]
    public void Success_ReturnsIsSuccessTrue_AndNoError()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailure);
        Assert.AreEqual(Error.None, result.Error);
    }

    [TestMethod]
    public void Failure_ReturnsIsSuccessFalse_AndError()
    {
        // Arrange
        var error = new Error("Test.Code", "Test description", "TargetProperty");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(error, result.Error);
        Assert.AreEqual("Test.Code", result.Error.Code);
        Assert.AreEqual("Test description", result.Error.Description);
        Assert.AreEqual("TargetProperty", result.Error.Target);
    }

    [TestMethod]
    public void SuccessT_ReturnsValue_AndIsSuccessTrue()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = Result.Success(value);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailure);
        Assert.AreEqual(value, result.Value);
        Assert.AreEqual(Error.None, result.Error);
    }

    [TestMethod]
    public void FailureT_ThrowsInvalidOperationException_WhenAccessingValue()
    {
        // Arrange
        var error = new Error("Test.Code", "Failure occurred");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(error, result.Error);
        Assert.ThrowsExactly<InvalidOperationException>(() => _ = result.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromValue_ReturnsSuccessResult()
    {
        // Arrange
        const int expectedValue = 42;

        // Act
        Result<int> result = expectedValue;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedValue, result.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromErrorToResultT_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("Error.Code", "Error message");

        // Act
        Result<string> result = error;

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(error, result.Error);
    }

    [TestMethod]
    public void ImplicitConversion_FromErrorToResult_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("Error.Code", "Error message");

        // Act
        Result result = error;

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(error, result.Error);
    }
}
