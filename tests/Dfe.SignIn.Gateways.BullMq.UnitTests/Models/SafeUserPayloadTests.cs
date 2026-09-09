using System.Text.Json;
using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Gateways.BullMq.Models;

namespace Dfe.SignIn.Gateways.BullMq.UnitTests.Models;

[TestClass]
public sealed class SafeUserPayloadTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [TestMethod]
    public void FromDomainEvent_MapsExpectedFieldsFromUserUpdatedEvent()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var @event = new UserUpdatedEvent {
            UserId = userId,
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = 1,
        };

        // Act
        var payload = SafeUserPayload.FromDomainEvent(@event);

        // Assert
        Assert.AreEqual(userId.ToString(), payload.Sub);
        Assert.AreEqual("user@example.com", payload.Email);
        Assert.AreEqual("Ada", payload.GivenName);
        Assert.AreEqual("Lovelace", payload.FamilyName);
        Assert.AreEqual((short)1, payload.Status);
    }

    [TestMethod]
    public void FromDomainEvent_LeavesUnmappedFieldsNull()
    {
        // Arrange
        var @event = new UserUpdatedEvent {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = 1,
        };

        // Act
        var payload = SafeUserPayload.FromDomainEvent(@event);

        // Assert
        Assert.IsNull(payload.LegacyUsername);
        Assert.IsNull(payload.PhoneNumber);
        Assert.IsNull(payload.LastLogin);
        Assert.IsNull(payload.PrevLogin);
        Assert.IsNull(payload.IsEntra);
        Assert.IsNull(payload.EntraOid);
        Assert.IsNull(payload.EntraLinked);
        Assert.IsNull(payload.EntraDeferUntil);
    }

    [TestMethod]
    public void FromDomainEvent_DefaultsIsInternalUserToFalse()
    {
        // Arrange
        var @event = new UserUpdatedEvent {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = 1,
        };

        // Act
        var payload = SafeUserPayload.FromDomainEvent(@event);

        // Assert — property default is false (not null), so it is included when serializing
        Assert.AreEqual(false, payload.IsInternalUser);
    }

    [TestMethod]
    public void Serialize_UsesLegacyPropertyNamesAndOmitsNullOptionals()
    {
        // Arrange
        var payload = new SafeUserPayload {
            Sub = "11111111-1111-1111-1111-111111111111",
            Email = "user@example.com",
            GivenName = "Ada",
            FamilyName = null,
            Status = 1,
            LegacyUsername = null,
            IsInternalUser = null,
        };

        // Act
        var json = JsonSerializer.Serialize(payload, SerializerOptions);

        // Assert
        Assert.Contains("\"sub\":", json);
        Assert.Contains("\"email\":", json);
        Assert.Contains("\"given_name\":\"Ada\"", json);
        Assert.Contains("\"status\":1", json);
        Assert.DoesNotContain("family_name", json);
        Assert.DoesNotContain("legacy_username", json);
        Assert.DoesNotContain("phone_number", json);
        Assert.DoesNotContain("isInternalUser", json);
        Assert.DoesNotContain("isEntra", json);
    }

    [TestMethod]
    public void Serialize_IncludesIsInternalUserFalse_WhenDefaulted()
    {
        // Arrange — FromDomainEvent leaves IsInternalUser at its default of false
        var payload = SafeUserPayload.FromDomainEvent(new UserUpdatedEvent {
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = 1,
        });

        // Act
        var json = JsonSerializer.Serialize(payload, SerializerOptions);

        // Assert — false is not null, so WhenWritingNull still emits the property
        Assert.Contains("\"isInternalUser\":false", json);
    }
}
