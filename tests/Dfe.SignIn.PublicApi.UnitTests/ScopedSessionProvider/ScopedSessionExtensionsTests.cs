namespace Dfe.SignIn.PublicApi.UnitTests.ScopedSessionProvider;

[TestClass]
public class ScopedSessionExtensionsTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupScopedSession_Throws_WhenServicesArgumentIsNull()
    {
        ScopedSessionExtensions.SetupScopedSession(
            services: null!
        );
    }
}