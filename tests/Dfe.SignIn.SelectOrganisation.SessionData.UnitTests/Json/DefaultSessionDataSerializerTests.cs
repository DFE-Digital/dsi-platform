using System.Text.Json;
using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.SelectOrganisation.SessionData.Json;

namespace Dfe.SignIn.SelectOrganisation.SessionData.UnitTests.Json;

[TestClass]
public sealed class DefaultSessionDataSerializerTests
{
    private static readonly SelectOrganisationSessionData FakeSessionData = new()
    {
        Created = new DateTime(2024, 2, 22),
        Expires = new DateTime(2024, 2, 22) + new TimeSpan(0, 10, 0),
        CallbackUrl = new Uri("https://example.localhost/callback"),
        ClientId = "example-client-id",
        UserId = new Guid("a205d032-e65f-47e0-810c-4ddb424219fd"),
        Prompt = new()
        {
            Heading = "Which organisation?",
            Hint = "Select one option.",
        },
        OrganisationOptions = [
            new() {
                Id = new Guid("cec48519-0b98-4bb4-8aaf-d597baed2e29"),
                Name = "Example organisation",
            },
        ],
    };

    #region Serialize(SelectOrganisationSessionData)

    [TestMethod]
    public void Serialize_Throws_WhenSessionDataArgumentIsNull()
    {
        var serializer = new DefaultSessionDataSerializer();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => serializer.Serialize(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void Serialize_ReturnsSerializedObject()
    {
        var serializer = new DefaultSessionDataSerializer();

        string result = serializer.Serialize(FakeSessionData);

        StringAssert.Contains(result, "\"clientId\":\"example-client-id\"");
    }

    #endregion

    #region Deserialize(string)

    [TestMethod]
    public void Deserialize_Throws_WhenJsonArgumentIsNull()
    {
        var serializer = new DefaultSessionDataSerializer();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => serializer.Deserialize(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    public void Deserialize_Throws_WhenJsonArgumentIsEmptyString()
    {
        var serializer = new DefaultSessionDataSerializer();

        Assert.ThrowsException<ArgumentException>(
            () => serializer.Deserialize("")
        );
    }

    [TestMethod]
    public void Deserialize_Throws_WhenInputIsInvalid()
    {
        var serializer = new DefaultSessionDataSerializer();

        Assert.ThrowsException<JsonException>(
            () => serializer.Deserialize("42")
        );
    }

    [TestMethod]
    public void Deserialize_Throws_WhenInputIsNull()
    {
        var serializer = new DefaultSessionDataSerializer();

        Assert.ThrowsException<JsonException>(
            () => serializer.Deserialize("null")
        );
    }

    [TestMethod]
    public void Deserialize_ReturnsDeserializedObject()
    {
        var serializer = new DefaultSessionDataSerializer();
        string json = serializer.Serialize(FakeSessionData);

        var result = serializer.Deserialize(json);

        Assert.AreEqual(FakeSessionData, result! with
        {
            OrganisationOptions = FakeSessionData.OrganisationOptions,
        });
        CollectionAssert.AreEqual(
            FakeSessionData.OrganisationOptions.ToArray(),
            result.OrganisationOptions.ToArray()
        );
    }

    #endregion
}
