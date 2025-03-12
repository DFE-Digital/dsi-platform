using Dfe.SignIn.PublicApi.Configuration;
using Dfe.SignIn.PublicApi.Endpoints.DigitalSigning;
using Microsoft.Extensions.Options;
using Moq;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.DigitalSigning;

[TestClass]
public sealed class GetKeysTests
{
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("   ")]
    public void GetKeys_Throws_WhenConfigurationIsMissing(string fakePublicKeysJsonValue)
    {
        var mockedOptionsAccessor = new Mock<IOptions<ApplicationOptions>>();
        mockedOptionsAccessor
            .Setup(x => x.Value)
            .Returns(new ApplicationOptions {
                PublicKeysJson = fakePublicKeysJsonValue,
            });

        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            DigitalSigningEndpoints.GetKeys(mockedOptionsAccessor.Object)
        );
        Assert.AreEqual(
            "Missing configuration 'ApplicationOptions.PublicKeysJson'.",
            ex.Message
        );
    }

    [TestMethod]
    public void GetKeys_ReturnsPublicKeysFromConfiguration()
    {
        var mockedOptionsAccessor = new Mock<IOptions<ApplicationOptions>>();
        mockedOptionsAccessor
            .Setup(x => x.Value)
            .Returns(new ApplicationOptions {
                PublicKeysJson = "Example",
            });

        var response = DigitalSigningEndpoints.GetKeys(mockedOptionsAccessor.Object);

        Assert.AreEqual("Example", response);
    }
}
