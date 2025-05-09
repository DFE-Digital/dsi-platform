using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;

namespace Dfe.SignIn.Gateways.SelectOrganisation.DistributedCache.UnitTests;

[TestClass]
public sealed class DefaultSessionDataSerializerTests
{
    private static readonly SelectOrganisationSessionData FakeSessionData = new() {
        Created = new DateTime(2024, 2, 22),
        Expires = new DateTime(2024, 2, 22) + new TimeSpan(0, 10, 0),
        ClientId = "example-client-id",
        UserId = new Guid("a205d032-e65f-47e0-810c-4ddb424219fd"),
        Prompt = new() {
            Heading = "Which organisation?",
            Hint = "Select one option.",
        },
        OrganisationOptions = [
            new() {
                Id = new Guid("cec48519-0b98-4bb4-8aaf-d597baed2e29"),
                Name = "Example organisation",
            },
        ],
        AllowCancel = true,
        CallbackUrl = new Uri("https://example.localhost/callback"),
        DetailLevel = OrganisationDetailLevel.Basic,
    };

    #region Serialize(SelectOrganisationSessionData)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Serialize_Throws_WhenSessionDataArgumentIsNull()
    {
        var serializer = new DefaultSessionDataSerializer();

        serializer.Serialize(
            sessionData: null!
        );
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
    [ExpectedException(typeof(ArgumentNullException))]
    public void Deserialize_Throws_WhenJsonArgumentIsNull()
    {
        var serializer = new DefaultSessionDataSerializer();

        serializer.Deserialize(
            json: null!
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Deserialize_Throws_WhenJsonArgumentIsEmptyString()
    {
        var serializer = new DefaultSessionDataSerializer();

        serializer.Deserialize(
            json: ""
        );
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_Throws_WhenInputIsInvalid()
    {
        var serializer = new DefaultSessionDataSerializer();

        serializer.Deserialize(
            json: "42"
        );
    }

    [TestMethod]
    [ExpectedException(typeof(JsonException))]
    public void Deserialize_Throws_WhenInputIsNull()
    {
        var serializer = new DefaultSessionDataSerializer();

        serializer.Deserialize(
            json: "null"
        );
    }

    [TestMethod]
    public void Deserialize_ReturnsDeserializedObject()
    {
        var serializer = new DefaultSessionDataSerializer();
        string json = serializer.Serialize(FakeSessionData);

        var result = serializer.Deserialize(json);

        Assert.AreEqual(FakeSessionData, result! with {
            OrganisationOptions = FakeSessionData.OrganisationOptions,
        });
        CollectionAssert.AreEqual(
            FakeSessionData.OrganisationOptions.ToArray(),
            result.OrganisationOptions.ToArray()
        );
    }

    #endregion
}
