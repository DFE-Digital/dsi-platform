namespace Dfe.SignIn.PublicApi.UnitTests.ScopedSessionProvider;

[TestClass]
public class ScopedSessionExtensionsTests
{
    [TestMethod]
    public void SetupScopedSession_Throws_WhenServicesArgumentIsNull()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.ThrowsException<ArgumentNullException>(
            () => ScopedSessionExtensions.SetupScopedSession(null)
        );
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}