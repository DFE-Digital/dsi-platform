using System.Text.Json;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Gateways.DistributedCache.Serialization;

namespace Dfe.SignIn.Gateways.DistributedCache.UnitTests.Serialization;

// Disable warning "Provide the "DateTimeKind" when creating this object."
// This is a unit test and it's unimportant.
#pragma warning disable S6562

[TestClass]
public sealed class DefaultCacheEntrySerializerTests
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
    };

    #region Serialize(SelectOrganisationSessionData)

    [TestMethod]
    public void Serialize_Throws_WhenSessionDataArgumentIsNull()
    {
        var serializer = new DefaultCacheEntrySerializer();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => serializer.Serialize<SelectOrganisationSessionData>(
                entry: null!
            ));
    }

    [TestMethod]
    public void Serialize_ReturnsSerializedObject()
    {
        var serializer = new DefaultCacheEntrySerializer();

        string result = serializer.Serialize(FakeSessionData);

        StringAssert.Contains(result, "\"clientId\":\"example-client-id\"");
    }

    #endregion

    #region Deserialize(string)

    [TestMethod]
    public void Deserialize_Throws_WhenJsonArgumentIsNull()
    {
        var serializer = new DefaultCacheEntrySerializer();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => serializer.Deserialize<SelectOrganisationSessionData>(
                entryJson: null!
            ));
    }

    [TestMethod]
    public void Deserialize_Throws_WhenJsonArgumentIsEmptyString()
    {
        var serializer = new DefaultCacheEntrySerializer();

        Assert.ThrowsExactly<ArgumentException>(()
            => serializer.Deserialize<SelectOrganisationSessionData>(
                entryJson: ""
            ));
    }

    [TestMethod]
    public void Deserialize_Throws_WhenInputIsInvalid()
    {
        var serializer = new DefaultCacheEntrySerializer();

        Assert.Throws<JsonException>(()
            => serializer.Deserialize<SelectOrganisationSessionData>(
                entryJson: "42"
            ));
    }

    [TestMethod]
    public void Deserialize_Throws_WhenInputIsNull()
    {
        var serializer = new DefaultCacheEntrySerializer();

        Assert.Throws<JsonException>(()
            => serializer.Deserialize<SelectOrganisationSessionData>(
                entryJson: "null"
            ));
    }

    [TestMethod]
    public void Deserialize_ReturnsDeserializedObject()
    {
        var serializer = new DefaultCacheEntrySerializer();
        string json = serializer.Serialize(FakeSessionData);

        var result = serializer.Deserialize<SelectOrganisationSessionData>(json);

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
