using System.Text.Json;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Gateways.BullMq.Models;

namespace Dfe.SignIn.Gateways.BullMq.UnitTests.Models;

[TestClass]
public sealed class SafeUserPayloadTests
{
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
        Assert.IsNull(payload.IsInternalUser);
        Assert.IsNull(payload.EntraDeferUntil);
    }

    [TestMethod]
    public void Serialize_WithDefaultOptions_UsesLegacyPropertyNamesAndOmitsNullOptionals()
    {
        // Arrange — default STJ options mirror BullMQ (property attributes alone drive naming/null omission)
        var payload = SafeUserPayload.FromDomainEvent(new UserUpdatedEvent {
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "user@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Status = 1,
        });

        // Act
        var json = JsonSerializer.Serialize(payload);

        // Assert
        Assert.Contains("\"sub\":", json);
        Assert.Contains("\"email\":", json);
        Assert.Contains("\"given_name\":\"Ada\"", json);
        Assert.Contains("\"family_name\":\"Lovelace\"", json);
        Assert.Contains("\"status\":1", json);
        Assert.DoesNotContain("legacy_username", json);
        Assert.DoesNotContain("phone_number", json);
        Assert.DoesNotContain("isInternalUser", json);
        Assert.DoesNotContain("isEntra", json);
    }

    [TestMethod]
    public void Serialize_WhenIsInternalUserNull_OmitsProperty()
    {
        // Arrange
        var payload = new SafeUserPayload {
            Sub = "11111111-1111-1111-1111-111111111111",
            Email = "user@example.com",
            GivenName = "Ada",
            FamilyName = null,
            Status = 1,
            IsInternalUser = null,
        };

        // Act
        var json = JsonSerializer.Serialize(payload);

        // Assert
        Assert.DoesNotContain("family_name", json);
        Assert.DoesNotContain("isInternalUser", json);
    }
}
