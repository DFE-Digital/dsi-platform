
using Dfe.SignIn.NodeApiClient.HttpSecurityProvider;
using Microsoft.Identity.Client;
using Moq;

namespace Dfe.SignIn.NodeApiClient.UnitTests.HttpSecurityProvider;

[TestClass]
public class MsalHttpSecurityProviderTests
{

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MsalHttpSecurityProvider_Throws_WhenScopesArgumentIsNull()
    {
        var mockIConfidentialClientApplication = new Mock<IConfidentialClientApplication>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        new MsalHttpSecurityProvider(null, mockIConfidentialClientApplication.Object);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void MsalHttpSecurityProvider_Throws_WhenConfidentialClientApplicationArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        new MsalHttpSecurityProvider([], null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
