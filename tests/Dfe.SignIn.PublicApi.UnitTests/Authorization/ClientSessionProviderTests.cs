using Dfe.SignIn.PublicApi.Authorization;

namespace Dfe.SignIn.PublicApi.UnitTests.Authorization;

[TestClass]
public sealed class ClientSessionProviderTests
{
    #region ClientId

    [TestMethod]
    public void ClientId_ReturnsTheValueThatWasAssigned()
    {
        var clientSessionProvider = new ClientSessionProvider();
        IClientSessionWriter writer = clientSessionProvider;
        IClientSession clientSession = clientSessionProvider;

        writer.ClientId = "test123";

        Assert.AreEqual("test123", clientSession.ClientId);
    }

    #endregion
}
