using System.IdentityModel.Tokens.Jwt;
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
                new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
                new(DsiClaimTypes.UserId, "350478c6-5d49-4060-b9d1-08bf216f2bc7"),
            ], "PrimaryAuthenticationType"),
        ]);

        var identity = user.GetPrimaryIdentity();

        Assert.IsNotNull(identity);
        Assert.AreEqual("PrimaryAuthenticationType", identity.AuthenticationType);
    }

    #endregion

    #region GetDsiIdentity(ClaimsPrincipal)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetDsiIdentity_Throws_When_UserArgumentIsNull()
    {
        DsiClaimExtensions.GetDsiIdentity(null!);
    }

    [TestMethod]
    public void GetDsiIdentity_ReturnsNull_WhenIdentityDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        var identity = user.GetDsiIdentity();

        Assert.IsNull(identity);
    }

    [TestMethod]
    public void GetDsiIdentity_ReturnsNull_WhenSessionIdDoesNotMatchPrimaryIdentity()
    {
        var user = new ClaimsPrincipal([
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
                new(DsiClaimTypes.UserId, "fa251e54-db87-4c74-9c72-9c9eb74ea046"),
            ], "PrimaryAuthenticationType"),
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, "41dcff2e-17a1-4920-ad10-0f77f7d2a17c"),
            ], PublicApiConstants.AuthenticationType),
        ]);

        var identity = user.GetDsiIdentity();

        Assert.IsNull(identity);
    }

    [TestMethod]
    public void GetDsiIdentity_ReturnsExpectedIdentity()
    {
        var user = new ClaimsPrincipal([
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
                new(DsiClaimTypes.UserId, "fa251e54-db87-4c74-9c72-9c9eb74ea046"),
            ], "PrimaryAuthenticationType"),
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
            ], PublicApiConstants.AuthenticationType),
        ]);

        var identity = user.GetDsiIdentity();

        Assert.IsNotNull(identity);
        Assert.AreEqual(PublicApiConstants.AuthenticationType, identity.AuthenticationType);
    }

    #endregion

    #region GetSessionId(ClaimsPrincipal, string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetSessionId_Throws_WhenUserArgumentIsNull()
    {
        DsiClaimExtensions.GetSessionId(null!);
    }

    [TestMethod]
    public void GetSessionId_Throws_WhenPrimaryIdentityDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        var exception = Assert.Throws<MissingClaimException>(
            () => user.GetSessionId()
        );
        Assert.AreEqual("Missing primary identity with claim.", exception.Message);
        Assert.AreEqual(DsiClaimTypes.SessionId, exception.ClaimType);
    }

    [TestMethod]
    public void GetSessionId_Throws_WhenPrimaryIdentityExistsButWithoutClaim()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(DsiClaimTypes.UserId, "37f6055b-7eca-46bc-bef6-a6882615bf0a"),
        ]));

        var exception = Assert.Throws<MissingClaimException>(
            () => user.GetSessionId()
        );
        Assert.AreEqual("Missing primary identity with claim.", exception.Message);
        Assert.AreEqual(DsiClaimTypes.SessionId, exception.ClaimType);
    }

    [TestMethod]
    public void GetSessionId_ReturnsClaim()
    {
        var primaryIdentity = new ClaimsIdentity([
            new Claim(DsiClaimTypes.UserId, "37f6055b-7eca-46bc-bef6-a6882615bf0a"),
            new(DsiClaimTypes.SessionId, "adce45ed-491c-4859-be55-847b23cfad1d"),
        ]);
        var user = new ClaimsPrincipal(primaryIdentity);

        var sessionId = user.GetSessionId();

        Assert.AreEqual("adce45ed-491c-4859-be55-847b23cfad1d", sessionId);
    }

    #endregion

    #region TryGetSessionId(ClaimsPrincipal, string, string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TryGetSessionId_Throws_WhenUserArgumentIsNull()
    {
        DsiClaimExtensions.TryGetSessionId(null!, out var _);
    }

    [TestMethod]
    public void TryGetSessionId_ReturnsFalse_WhenPrimaryIdentityDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        bool result = user.TryGetSessionId(out string? sessionId);

        Assert.IsFalse(result);
        Assert.IsNull(sessionId);
    }

    [TestMethod]
    public void TryGetSessionId_ReturnsFalse_WhenPrimaryIdentityExistsButWithoutClaim()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(DsiClaimTypes.UserId, "37f6055b-7eca-46bc-bef6-a6882615bf0a"),
        ]));

        bool result = user.TryGetSessionId(out string? sessionId);

        Assert.IsFalse(result);
        Assert.IsNull(sessionId);
    }

    [TestMethod]
    public void TryGetSessionId_ReturnsTrue_WhenPrimaryIdentityExistsWithClaim()
    {
        var primaryIdentity = new ClaimsIdentity([
            new(DsiClaimTypes.SessionId, "adce45ed-491c-4859-be55-847b23cfad1d"),
            new Claim(DsiClaimTypes.UserId, "37f6055b-7eca-46bc-bef6-a6882615bf0a"),
        ]);
        var user = new ClaimsPrincipal(primaryIdentity);

        bool result = user.TryGetSessionId(out string? sessionId);

        Assert.IsTrue(result);
        Assert.AreEqual("adce45ed-491c-4859-be55-847b23cfad1d", sessionId);
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
            new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
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
            new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
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
            new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
            new(DsiClaimTypes.UserId, "58eb2690-5266-4cbb-ab46-1f4a211ce9c0"),
        ], "PrimaryAuthenticationType");
        var user = new ClaimsPrincipal(identity);

        user.TryGetDsiUserId(out var output);

        Assert.AreEqual(new Guid("58eb2690-5266-4cbb-ab46-1f4a211ce9c0"), output);
    }

    #endregion

    #region GetDsiOrganisation(ClaimsPrincipal)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetDsiOrganisation_Throws_WhenUserArgumentIsNull()
    {
        DsiClaimExtensions.GetDsiOrganisation(null!);
    }

    [TestMethod]
    public void GetDsiOrganisation_ReturnsNull_WhenClaimDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        var result = user.GetDsiOrganisation();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetDsiOrganisation_ReturnsClaimAsObject_WhenClaimDoesExist()
    {
        var user = new ClaimsPrincipal([
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
                new(DsiClaimTypes.UserId, "fa251e54-db87-4c74-9c72-9c9eb74ea046"),
            ], "PrimaryAuthenticationType"),
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
                new(DsiClaimTypes.Organisation, /*lang=json,strict*/ """
                    {
                        "id": "4db99a83-60ac-4e3f-b87f-81d384f673e7",
                        "name": "Example organisation name"
                    }
                """, JsonClaimValueTypes.Json),
            ], PublicApiConstants.AuthenticationType),
        ]);

        var result = user.GetDsiOrganisation();

        var expectedResult = new OrganisationClaim {
            Id = new Guid("4db99a83-60ac-4e3f-b87f-81d384f673e7"),
            Name = "Example organisation name",
        };
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    public void GetDsiOrganisation_ReturnsNull_WhenSessionIdDoesNotMatchPrimaryIdentity()
    {
        var user = new ClaimsPrincipal([
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, "460d68fe-6887-401d-a218-6e455deea824"),
                new(DsiClaimTypes.UserId, "fa251e54-db87-4c74-9c72-9c9eb74ea046"),
            ], "PrimaryAuthenticationType"),
            new ClaimsIdentity([
                new(DsiClaimTypes.SessionId, "41dcff2e-17a1-4920-ad10-0f77f7d2a17c"),
                new(DsiClaimTypes.Organisation, /*lang=json,strict*/ """
                    {
                        "id": "4db99a83-60ac-4e3f-b87f-81d384f673e7",
                        "name": "Example organisation name"
                    }
                """, JsonClaimValueTypes.Json),
            ], PublicApiConstants.AuthenticationType),
        ]);

        var identity = user.GetDsiOrganisation();

        Assert.IsNull(identity);
    }

    #endregion
}
