using Dfe.SignIn.PublicApi.Configuration;
using Dfe.SignIn.PublicApi.Endpoints.DigitalSigning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.DigitalSigning;

[TestClass]
public sealed class GetKeysTests
{
    private static IOptions<ApplicationOptions> GetApplicationOptionsWithMockedPublicJsonKeys(
        string publicKeysJson = "{json data...}")
    {
        var mockedOptionsAccessor = new Mock<IOptions<ApplicationOptions>>();
        mockedOptionsAccessor
            .Setup(x => x.Value)
            .Returns(new ApplicationOptions {
                PublicKeysJson = publicKeysJson,
            });
        return mockedOptionsAccessor.Object;
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("   ")]
    public void GetKeys_Throws_WhenConfigurationIsMissing(string fakePublicKeysJsonValue)
    {
        var applicationOptions = GetApplicationOptionsWithMockedPublicJsonKeys(
            fakePublicKeysJsonValue
        );
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            DigitalSigningEndpoints.GetKeys(applicationOptions)
        );
        Assert.AreEqual(
            "Missing configuration 'ApplicationOptions.PublicKeysJson'.",
            ex.Message
        );
    }

    [TestMethod]
    public void GetKeys_ReturnsPublicKeysFromConfiguration()
    {
        var applicationOptions = GetApplicationOptionsWithMockedPublicJsonKeys();

        var result = DigitalSigningEndpoints.GetKeys(applicationOptions);

        Assert.IsInstanceOfType<ContentHttpResult>(result);
        var contentResult = (ContentHttpResult)result;
        Assert.AreEqual("{json data...}", contentResult.ResponseContent);
    }

    [TestMethod]
    public void GetKeys_ReturnsExpectedContentType()
    {
        var applicationOptions = GetApplicationOptionsWithMockedPublicJsonKeys();

        var result = DigitalSigningEndpoints.GetKeys(applicationOptions);

        Assert.IsInstanceOfType<ContentHttpResult>(result);
        var contentResult = (ContentHttpResult)result;
        Assert.AreEqual("application/json", contentResult.ContentType);
    }
}
