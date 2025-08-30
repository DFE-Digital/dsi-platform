
using Dfe.SignIn.NodeApi.Client.HttpSecurityProvider;
using Microsoft.Identity.Client;
using Moq;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.HttpSecurityProvider;

[TestClass]
public sealed class MsalHttpSecurityProviderTests
{
    [TestMethod]
    public void MsalHttpSecurityProvider_Throws_WhenScopesArgumentIsNull()
    {
        var mockIConfidentialClientApplication = new Mock<IConfidentialClientApplication>();

        Assert.ThrowsExactly<ArgumentNullException>(()
            => new MsalHttpSecurityProvider(
                scopes: null!,
                confidentialClientApplication: mockIConfidentialClientApplication.Object
            ));
    }

    [TestMethod]
    public void MsalHttpSecurityProvider_Throws_WhenConfidentialClientApplicationArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => new MsalHttpSecurityProvider(
                scopes: [],
                confidentialClientApplication: null!
            ));
    }
}
