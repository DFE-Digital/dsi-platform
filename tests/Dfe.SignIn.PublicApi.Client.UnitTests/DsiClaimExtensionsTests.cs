using System.Security.Claims;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class DsiClaimExtensionsTests
{
    #region GetPrimaryIdentity(ClaimsPrincipal)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetPrimaryIdentity_Throws_When_UserArgumentIsNull()
    {
        DsiClaimExtensions.GetPrimaryIdentity(null!);
    }

    [TestMethod]
    public void GetPrimaryIdentity_ReturnsNull_WhenIdentityDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        var identity = user.GetPrimaryIdentity();

        Assert.IsNull(identity);
    }

    [TestMethod]
    public void GetPrimaryIdentity_ReturnsExpectedIdentity()
    {
        var user = new ClaimsPrincipal([
            new ClaimsIdentity("SomeOtherAuthenticationType"),
            new ClaimsIdentity([
                new(DsiClaimTypes.UserId, "350478c6-5d49-4060-b9d1-08bf216f2bc7"),
            ], "PrimaryAuthenticationType"),
        ]);

        var identity = user.GetPrimaryIdentity();

        Assert.IsNotNull(identity);
        Assert.AreEqual("PrimaryAuthenticationType", identity.AuthenticationType);
    }

    #endregion

    #region GetDsiUserId(ClaimsPrincipal)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetDsiUserId_Throws_WhenUserArgumentIsNull()
    {
        DsiClaimExtensions.GetDsiUserId(null!);
    }

    [TestMethod]
    public void GetDsiUserId_Throws_WhenClaimDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        var exception = Assert.Throws<MissingClaimException>(
            () => user.GetDsiUserId()
        );
        Assert.AreEqual("Missing identity with claim.", exception.Message);
        Assert.AreEqual(DsiClaimTypes.UserId, exception.ClaimType);
    }

    [TestMethod]
    public void GetDsiUserId_ReturnsClaimAsGuid()
    {
        var identity = new ClaimsIdentity((IEnumerable<Claim>?)[
            new(DsiClaimTypes.UserId, "58eb2690-5266-4cbb-ab46-1f4a211ce9c0"),
        ], "PrimaryAuthenticationType");
        var user = new ClaimsPrincipal(identity);

        var result = user.GetDsiUserId();

        Assert.AreEqual(new Guid("58eb2690-5266-4cbb-ab46-1f4a211ce9c0"), result);
    }

    #endregion

    #region TryGetDsiUserId(ClaimsPrincipal, out Guid?)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TryGetDsiUserId_Throws_WhenUserArgumentIsNull()
    {
        DsiClaimExtensions.TryGetDsiUserId(null!, out _);
    }

    [TestMethod]
    public void TryGetDsiUserId_ReturnsFalse_WhenClaimDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        bool result = user.TryGetDsiUserId(out _);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void TryGetDsiUserId_OutputsNull_WhenClaimDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        user.TryGetDsiUserId(out var output);

        Assert.IsNull(output);
    }

    [TestMethod]
    public void TryGetDsiUserId_ReturnsTrue_WhenClaimDoesExist()
    {
        var identity = new ClaimsIdentity((IEnumerable<Claim>?)[
            new(DsiClaimTypes.UserId, "58eb2690-5266-4cbb-ab46-1f4a211ce9c0"),
        ], "PrimaryAuthenticationType");
        var user = new ClaimsPrincipal(identity);

        bool result = user.TryGetDsiUserId(out _);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TryGetDsiUserId_OutputsClaim_WhenClaimDoesExist()
    {
        var identity = new ClaimsIdentity((IEnumerable<Claim>?)[
            new(DsiClaimTypes.UserId, "58eb2690-5266-4cbb-ab46-1f4a211ce9c0"),
        ], "PrimaryAuthenticationType");
        var user = new ClaimsPrincipal(identity);

        user.TryGetDsiUserId(out var output);

        Assert.AreEqual(new Guid("58eb2690-5266-4cbb-ab46-1f4a211ce9c0"), output);
    }

    #endregion
}
