using Dfe.SignIn.InternalApi.Configuration;
using Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Services;
using Microsoft.Extensions.Options;
using Xunit.Sdk;

namespace Dfe.SignIn.InternalApi.UnitTests.Features.Users;

[TestClass]
public class GenericEmailCheckerTests
{
    [TestMethod]
    public void IsEmailGeneric_ReturnsTrue_WhenEmailExistsInList()
    {
        var options = Options.Create(new EmailRestrictions {
            GenericEmailStrings = ["admin", "adminoffice"]
        });

        var sut = new GenericEmailCheck(options);

        var result = sut.IsEmailGeneric("admin");

        Assert.IsTrue(result);
    }

    [TestMethod]

    public void IsEmailGeneric_ReturnsFalse_WhenEmailDoesNotExistInList()
    {
        // Arrange
        var options = Options.Create(new EmailRestrictions {
            GenericEmailStrings = ["admin", "adminoffice"]
        });

        var sut = new GenericEmailCheck(options);

        // Act
        var result = sut.IsEmailGeneric("fred");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsEmailGeneric_ReturnsFalse_WhenConfigurationValueMissing()
    {
        // Arrange
        var options = Options.Create(new EmailRestrictions());

        var sut = new GenericEmailCheck(options);

        // Act
        var result = sut.IsEmailGeneric("admin");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsEmailGeneric_ReturnsFalse_WhenListIsEmpty()
    {
        // Arrange
        var options = Options.Create(new EmailRestrictions {
            GenericEmailStrings = []
        });

        var sut = new GenericEmailCheck(options);

        // Act
        var result = sut.IsEmailGeneric("admin");

        // Assert
        Assert.IsFalse(result);

    }
}
