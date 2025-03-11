namespace Dfe.SignIn.PublicApi.UnitTests.ScopedSessionProvider;

[TestClass]
public class ScopedSessionExtensionsTests
{
    [TestMethod]
    public void SetupScopedSession_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            ScopedSessionExtensions.SetupScopedSession(
                services: null!
            )
        );
    }
}