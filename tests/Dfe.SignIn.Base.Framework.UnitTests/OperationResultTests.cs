using Dfe.SignIn.Base.Framework.OperationResults;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class OperationResultTests
{
    [TestMethod]
    public void Success_ReturnsIsSuccessTrue_AndNoError()
    {
        // Act
        var result = OperationResult.Success();

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailure);
        Assert.AreEqual(OperationError.None, result.Error);
    }

    [TestMethod]
    public void Failure_ReturnsIsSuccessFalse_AndError()
    {
        // Arrange
        var error = new OperationError("Test.Code", "Test description", "TargetProperty");

        // Act
        var result = OperationResult.Failure(error);

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
        var result = OperationResult.Success(value);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.IsFailure);
        Assert.AreEqual(value, result.Value);
        Assert.AreEqual(OperationError.None, result.Error);
    }

    [TestMethod]
    public void FailureT_ThrowsInvalidOperationException_WhenAccessingValue()
    {
        // Arrange
        var error = new OperationError("Test.Code", "Failure occurred");

        // Act
        var result = OperationResult.Failure<string>(error);

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
        OperationResult<int> result = expectedValue;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expectedValue, result.Value);
    }

    [TestMethod]
    public void ImplicitConversion_FromErrorToResultT_ReturnsFailureResult()
    {
        // Arrange
        var error = new OperationError("Error.Code", "Error message");

        // Act
        OperationResult<string> result = error;

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(error, result.Error);
    }

    [TestMethod]
    public void ImplicitConversion_FromErrorToResult_ReturnsFailureResult()
    {
        // Arrange
        var error = new OperationError("Error.Code", "Error message");

        // Act
        OperationResult result = error;

        // Assert
        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(error, result.Error);
    }
}
