using System.Text.Json;
using Dfe.SignIn.Base.Framework.UnitTests.Fakes;
using Moq.AutoMock;

namespace Dfe.SignIn.Base.Framework.UnitTests;

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

    private static void AssertNoUnwantedProperties(JsonElement jsonElement)
    {
        Assert.IsFalse(jsonElement.TryGetProperty("trace", out var _));
        Assert.IsFalse(jsonElement.TryGetProperty("stackTrace", out var _));
        Assert.IsFalse(jsonElement.TryGetProperty("source", out var _));
        Assert.IsFalse(jsonElement.TryGetProperty("inner", out var _));
        Assert.IsFalse(jsonElement.TryGetProperty("innerException", out var _));
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

        var jsonElement = serializer.SerializeExceptionToJson(exception);

        Assert.AreEqual("System.Exception", jsonElement.GetProperty("type").GetString());
        Assert.AreEqual(exception.Message, jsonElement.GetProperty("message").GetString());
        AssertNoUnwantedProperties(jsonElement);
    }

    [TestMethod]
    public void SerializeExceptionToJson_IgnoresInnerException()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var innerException = new Exception("Example inner exception message.", null);
        innerException = AddFakeStackTraceToException(innerException);

        var exception = new UnexpectedException("An unexpected error has occurred.", innerException);
        exception = AddFakeStackTraceToException(exception);

        var jsonElement = serializer.SerializeExceptionToJson(exception);

        Assert.AreEqual(
            "Dfe.SignIn.Base.Framework.UnexpectedException",
            jsonElement.GetProperty("type").GetString()
        );
        Assert.AreEqual(
            exception.Message,
            jsonElement.GetProperty("message").GetString()
        );
        AssertNoUnwantedProperties(jsonElement);
    }

    [TestMethod]
    public void SerializeExceptionToJson_CanSerializeCustomExceptionType()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var exception = new FakeInteractionException("Example exception message.");
        exception = AddFakeStackTraceToException(exception);

        var jsonElement = serializer.SerializeExceptionToJson(exception);

        Assert.AreEqual(
            "Dfe.SignIn.Base.Framework.UnitTests.Fakes.FakeInteractionException",
            jsonElement.GetProperty("type").GetString()
        );
        Assert.AreEqual(exception.Message, jsonElement.GetProperty("message").GetString());
        AssertNoUnwantedProperties(jsonElement);
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

        var jsonElement = serializer.SerializeExceptionToJson(exception);

        Assert.AreEqual(
            "Dfe.SignIn.Base.Framework.UnitTests.Fakes.FakeInteractionExceptionWithPeristedProperties",
            jsonElement.GetProperty("type").GetString()
        );
        Assert.AreEqual(exception.Message, jsonElement.GetProperty("message").GetString());
        Assert.AreEqual(exception.CustomProperty, jsonElement.GetProperty("customProperty").GetString());
        AssertNoUnwantedProperties(jsonElement);
        Assert.IsFalse(jsonElement.TryGetProperty("computedProperty", out var _));
    }

    [TestMethod]
    public void SerializeExceptionToJson_ConsistentCasingInValidationResults()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        var exception = new InvalidRequestException(
            invocationId: new Guid("a755e30d-8850-4a0b-817a-538cb36b4748"),
            validationResults: [
                new("Example error message.", [ "MemberName1", "MemberName2" ]),
            ]
        );
        exception = AddFakeStackTraceToException(exception);

        var jsonElement = serializer.SerializeExceptionToJson(exception);

        var validationResultsElement = jsonElement.GetProperty("validationResults");
        Assert.AreEqual(JsonValueKind.Array, validationResultsElement.ValueKind);

        var validationResultElement = validationResultsElement.EnumerateArray().First();
        Assert.AreEqual(JsonValueKind.Object, validationResultElement.ValueKind);
        Assert.AreEqual("Example error message.", validationResultElement.GetProperty("errorMessage").GetString());
        var memberNames = validationResultElement.GetProperty("memberNames").EnumerateArray().Select(x => x.GetString()).ToArray();
        CollectionAssert.AreEqual(new string[] { "memberName1", "memberName2" }, memberNames);
    }

    #endregion

    #region DeserializeExceptionFromJson(string)

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsUnexpectedException_WhenUnexpectedValueType()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        string json = /*lang=json,strict*/ "null";
        JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

        var result = serializer.DeserializeExceptionFromJson(jsonElement);

        Assert.IsInstanceOfType<UnexpectedException>(result);
        Assert.AreEqual("Unknown exception type.", result.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsUnexpectedException_WhenExceptionTypeIsUnknown()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        string json = /*lang=json,strict*/ """
        {
            "type": "NonExistentException",
            "message": "An example of an exception that does not exist.",
            "customProperty": "A custom property.",
            "computedProperty": "Attempting to deserialize to a computed property..."
        }
        """;
        JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

        var result = serializer.DeserializeExceptionFromJson(jsonElement);

        Assert.IsInstanceOfType<UnexpectedException>(result);
        var exception = (UnexpectedException)result;

        Assert.AreEqual("An example of an exception that does not exist.", exception.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsUnexpectedException_WhenExceptionConstructorIsMissing()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        string json = /*lang=json,strict*/ """
        {
            "type": "Dfe.SignIn.Base.Framework.UnitTests.Fakes.FakeInteractionExceptionWithMissingConstructor",
            "message": "An example of an exception with missing constructors."
        }
        """;
        JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

        var result = serializer.DeserializeExceptionFromJson(jsonElement);

        Assert.IsInstanceOfType<UnexpectedException>(result);
        var exception = (UnexpectedException)result;

        Assert.AreEqual("An example of an exception with missing constructors.", exception.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReturnsExpectedException()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        string json = /*lang=json,strict*/ """
        {
            "type": "System.Exception",
            "message": "An example exception."
        }
        """;
        JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

        var result = serializer.DeserializeExceptionFromJson(jsonElement);

        Assert.IsInstanceOfType<Exception>(result);
        Assert.AreEqual("An example exception.", result.Message);
    }

    [TestMethod]
    public void DeserializeExceptionFromJson_ReadsPersistedProperties()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        string json = /*lang=json,strict*/ """
        {
            "type": "Dfe.SignIn.Base.Framework.UnitTests.Fakes.FakeInteractionExceptionWithPeristedProperties",
            "message": "An example exception.",
            "customProperty": "A custom property.",
            "computedProperty": "Attempting to deserialize to a computed property..."
        }
        """;
        JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

        var result = serializer.DeserializeExceptionFromJson(jsonElement);

        var exception = TypeAssert.IsType<FakeInteractionExceptionWithPeristedProperties>(result);

        Assert.AreEqual("An example exception.", exception.Message);
        Assert.AreEqual("A custom property.", exception.CustomProperty);
        Assert.AreEqual("Computed: A custom property.", exception.ComputedProperty);
    }

    [TestMethod]
    public void DeserializeExceptionToJson_ConsistentCasingInValidationResults()
    {
        var serializer = CreateDefaultExceptionJsonSerializer();

        string json = /*lang=json,strict*/ """
        {
            "type": "Dfe.SignIn.Base.Framework.InvalidRequestException",
            "message": "An example exception.",
            "validationResults": [
                {
                    "errorMessage": "An example validation error.",
                    "memberNames": [ "memberName1", "memberName2" ]
                }
            ]
        }
        """;
        JsonElement jsonElement = JsonDocument.Parse(json).RootElement;

        var result = serializer.DeserializeExceptionFromJson(jsonElement);

        var exception = TypeAssert.IsType<InvalidRequestException>(result);

        Assert.AreEqual("An example exception.", exception.Message);
        Assert.HasCount(1, exception.ValidationResults);

        var validationResult = exception.ValidationResults.First();
        Assert.AreEqual("An example validation error.", validationResult.ErrorMessage);
        CollectionAssert.AreEqual(new string[] { "MemberName1", "MemberName2" }, validationResult.MemberNames.ToArray());
    }

    #endregion
}
