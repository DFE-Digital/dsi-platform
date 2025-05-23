
using Dfe.SignIn.NodeApi.Client.HttpSecurityProvider;
using Microsoft.Identity.Client;
using Moq;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.HttpSecurityProvider;

[TestClass]
public sealed class MsalHttpSecurityProviderTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MsalHttpSecurityProvider_Throws_WhenScopesArgumentIsNull()
    {
        var mockIConfidentialClientApplication = new Mock<IConfidentialClientApplication>();

        new MsalHttpSecurityProvider(
            scopes: null!,
            confidentialClientApplication: mockIConfidentialClientApplication.Object
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MsalHttpSecurityProvider_Throws_WhenConfidentialClientApplicationArgumentIsNull()
    {
        new MsalHttpSecurityProvider(
            scopes: [],
            confidentialClientApplication: null!
        );
    }
}
