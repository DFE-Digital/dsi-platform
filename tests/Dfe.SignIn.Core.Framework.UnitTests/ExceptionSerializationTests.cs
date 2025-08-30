using System.Text.Json;
using Dfe.SignIn.Core.Framework.UnitTests.Fakes;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class ExceptionSerialization
{
    private static TException AddFakeStackTraceToException<TException>(TException exception)
        where TException : Exception
    {
        try {
            // Must throw exception so that it gets a stack trace.
            throw exception;
        }
        catch (TException ex) {
            return ex;
        }
    }

    private static DefaultExceptionJsonSerializer CreateDefaultExceptionJsonSerializer()
    {
        var autoMocker = new AutoMocker();
        autoMocker.UseStandardJsonSerializerOptions();
        return autoMocker.CreateInstance<DefaultExceptionJsonSerializer>();
    }

    #region SerializeExceptionToJson(Exception)

    private static void AssertNoUnwantedProperties(JsonDocument jsonDocument)
    {
        Assert.IsFalse(jsonDocument.RootElement.TryGetProperty("trace", out var _));
        Assert.IsFalse(jsonDocument.RootElement.TryGetProperty("stackTrace", out var _));
        Assert.IsFalse(jsonDocument.RootElement.TryGetProperty("source", out var _));
        Assert.IsFalse(jsonDocument.RootElement.TryGetProperty("inner", out var _));
        Assert.IsFalse(jsonDocument.RootElement.TryGetProperty("innerException", out var _));
    }

    [TestMethod]
    public void SerializeExceptionToJson_Throws_WhenExceptionArgumentIsNull()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => serializer.SerializeExceptionToJson(null!));
    }

    [TestMethod]
    public void SerializeExceptionToJson_CanSerializeSystemExceptionType()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var exception = new Exception("Example exception message.");
        exception = AddFakeStackTraceToException(exception);

        string json = serializer.SerializeExceptionToJson(exception);

        using var jsonDocument = JsonDocument.Parse(json);
        Assert.AreEqual("System.Exception", jsonDocument.RootElement.GetProperty("type").GetString());
        Assert.AreEqual(exception.Message, jsonDocument.RootElement.GetProperty("message").GetString());
        AssertNoUnwantedProperties(jsonDocument);
    }

    [TestMethod]
    public void SerializeExceptionToJson_IgnoresInnerException()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var innerException = new Exception("Example inner exception message.", null);
        innerException = AddFakeStackTraceToException(innerException);

        var exception = new UnexpectedException("An unexpected error has occurred.", innerException);
        exception = AddFakeStackTraceToException(exception);

        string json = serializer.SerializeExceptionToJson(exception);

        using var jsonDocument = JsonDocument.Parse(json);

        Assert.AreEqual(
            "Dfe.SignIn.Core.Framework.UnexpectedException",
            jsonDocument.RootElement.GetProperty("type").GetString()
        );
        Assert.AreEqual(
            exception.Message,
            jsonDocument.RootElement.GetProperty("message").GetString()
        );
        AssertNoUnwantedProperties(jsonDocument);
    }

    [TestMethod]
    public void SerializeExceptionToJson_CanSerializeCustomExceptionType()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var exception = new FakeInteractionException("Example exception message.");
        exception = AddFakeStackTraceToException(exception);

        string json = serializer.SerializeExceptionToJson(exception);

        using var jsonDocument = JsonDocument.Parse(json);
        Assert.AreEqual(
            "Dfe.SignIn.Core.Framework.UnitTests.Fakes.FakeInteractionException",
            jsonDocument.RootElement.GetProperty("type").GetString()
        );
        Assert.AreEqual(exception.Message, jsonDocument.RootElement.GetProperty("message").GetString());
        AssertNoUnwantedProperties(jsonDocument);
    }

    [TestMethod]
    public void SerializeExceptionToJson_WritesPersistedProperties()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var exception = new FakeInteractionExceptionWithPeristedProperties(
            message: "Example exception message.",
            customProperty: "Custom property."
        );
        exception = AddFakeStackTraceToException(exception);

        string json = serializer.SerializeExceptionToJson(exception);

        using var jsonDocument = JsonDocument.Parse(json);
        Assert.AreEqual(
            "Dfe.SignIn.Core.Framework.UnitTests.Fakes.FakeInteractionExceptionWithPeristedProperties",
            jsonDocument.RootElement.GetProperty("type").GetString()
        );
        Assert.AreEqual(exception.Message, jsonDocument.RootElement.GetProperty("message").GetString());
        Assert.AreEqual(exception.CustomProperty, jsonDocument.RootElement.GetProperty("customProperty").GetString());
        AssertNoUnwantedProperties(jsonDocument);
        Assert.IsFalse(jsonDocument.RootElement.TryGetProperty("computedProperty", out var _));
    }

    #endregion

    #region DeserializeExceptionFromJson(string)

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsUnexpectedException_WhenJsonArgumentIsNull()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var result = serializer.DeserializeExceptionFromJson(null!);

        Assert.IsInstanceOfType<UnexpectedException>(result);
        Assert.AreEqual("Unknown exception type.", result.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsUnexpectedException_WhenJsonArgumentIsEmptyString()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var result = serializer.DeserializeExceptionFromJson("");

        Assert.IsInstanceOfType<UnexpectedException>(result);
        Assert.AreEqual("Unknown exception type.", result.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsUnexpectedException_WhenExceptionTypeIsUnknown()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var result = serializer.DeserializeExceptionFromJson(/*lang=json,strict*/ """
            {
              "type": "NonExistentException",
              "message": "An example of an exception that does not exist.",
              "customProperty": "A custom property.",
              "computedProperty": "Attempting to deserialize to a computed property..."
            }
        """);

        Assert.IsInstanceOfType<UnexpectedException>(result);
        var exception = (UnexpectedException)result;

        Assert.AreEqual("An example of an exception that does not exist.", exception.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsUnexpectedException_WhenExceptionConstructorIsMissing()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var result = serializer.DeserializeExceptionFromJson(/*lang=json,strict*/ """
            {
              "type": "Dfe.SignIn.Core.Framework.UnitTests.Fakes.FakeInteractionExceptionWithMissingConstructor",
              "message": "An example of an exception with missing constructors."
            }
        """);

        Assert.IsInstanceOfType<UnexpectedException>(result);
        var exception = (UnexpectedException)result;

        Assert.AreEqual("An example of an exception with missing constructors.", exception.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsExpectedException()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var result = serializer.DeserializeExceptionFromJson(/*lang=json,strict*/ """
            {
              "type": "System.Exception",
              "message": "An example exception."
            }
        """);

        Assert.IsInstanceOfType<Exception>(result);
        Assert.AreEqual("An example exception.", result.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReadsPersistedProperties()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var result = serializer.DeserializeExceptionFromJson(/*lang=json,strict*/ """
            {
              "type": "Dfe.SignIn.Core.Framework.UnitTests.Fakes.FakeInteractionExceptionWithPeristedProperties",
              "message": "An example exception.",
              "customProperty": "A custom property.",
              "computedProperty": "Attempting to deserialize to a computed property..."
            }
        """);

        Assert.IsInstanceOfType<FakeInteractionExceptionWithPeristedProperties>(result);
        var exception = (FakeInteractionExceptionWithPeristedProperties)result;

        Assert.AreEqual("An example exception.", exception.Message);
        Assert.AreEqual("A custom property.", exception.CustomProperty);
        Assert.AreEqual("Computed: A custom property.", exception.ComputedProperty);
    }

    #endregion
}
