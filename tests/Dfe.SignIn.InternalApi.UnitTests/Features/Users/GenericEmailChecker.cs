using Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Services;
using Microsoft.Extensions.Configuration;
using Xunit.Sdk;

namespace Dfe.SignIn.InternalApi.UnitTests.Features.Users;

[TestClass]
public class GenericEmailCheckerTests
{
    [TestMethod]
    public void IsEmailGeneric_ReturnsTrue_WhenEmailExistsInList()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["GENERIC_EMAIL_STRINGS"] = "[\"admin\",\"adminoffice\"]"
            }).Build();

        var sut = new GenericEmailCheck(configuration);

        var result = sut.IsEmailGeneric("admin");

        Assert.IsTrue(result);
    }

    [TestMethod]

    public void IsEmailGeneric_ReturnsFalse_WhenEmailDoesNotExistInList()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> {
            ["GENERIC_EMAIL_STRINGS"] = "[\"admin\",\"adminoffice\"]"
        }).Build();

        var sut = new GenericEmailCheck(configuration);

        // Act
        var result = sut.IsEmailGeneric("fred");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsEmailGeneric_ReturnsFalse_WhenConfigurationValueMissing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var sut = new GenericEmailCheck(configuration);

        // Act
        var result = sut.IsEmailGeneric("admin");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsEmailGeneric_ReturnsFalse_WhenListIsEmpty()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["GENERIC_EMAIL_STRINGS"] = "[]"
            })
            .Build();

        var sut = new GenericEmailCheck(configuration);

        // Act
        var result = sut.IsEmailGeneric("admin");

        // Assert
        Assert.IsFalse(result);

    }
}
