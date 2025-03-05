using Dfe.SignIn.NodeApiClient.ConfidentialClientApplication;

namespace Dfe.SignIn.NodeApiClient.Tests.ConfidentialClientApplication;

[TestClass]
public class ConfidentialClientApplicationManagerTests
{

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConfidentialClientApplicationManager_Throws_WhenNodeNameApiArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        new ConfidentialClientApplicationManager(null, new NodeApiAuthenticatedHttpClientOptions());
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConfidentialClientApplicationManager_Throws_WhenOptionsArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        new ConfidentialClientApplicationManager([], null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }

    // [TestMethod]
    // public async Task Bob()
    // {
    //     static AuthenticationResult CreateAuthenticationResult(string accessToken)
    //     {
    //         return new AuthenticationResult(accessToken, true, null, DateTimeOffset.Now, DateTimeOffset.Now, string.Empty, null, null, null, Guid.Empty);
    //     }
    //     var app = new Mock<IConfidentialClientApplication>();
    //     AuthenticationResult auth = CreateAuthenticationResult("bob");
    //     app.Setup(c => c.AcquireTokenForClient(It.IsAny<string[]>()).ExecuteAsync())
    //         .ReturnsAsync(auth);

    //     ConfidentialClientApplicationManager appp = new([NodeApiName.Access], new NodeApiAuthenticatedHttpClientOptions { });

    //     var msg = new HttpRequestMessage();

    //     await appp.AddAuthorizationAsync(NodeApiName.Access, msg);
    // }
}