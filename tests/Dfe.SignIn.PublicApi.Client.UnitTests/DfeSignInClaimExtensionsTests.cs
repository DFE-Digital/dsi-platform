using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class DfeSignInClaimExtensionsTests
{
    #region GetDsiUserId(ClaimsPrincipal)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetDsiUserId_Throws_WhenUserArgumentIsNull()
    {
        DfeSignInClaimExtensions.GetDsiUserId(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetDsiUserId_Throws_WhenClaimDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        user.GetDsiUserId();
    }

    [TestMethod]
    public void GetDsiUserId_ReturnsClaimAsGuid()
    {
        var identity = new ClaimsIdentity([
            new(DfeSignInClaimTypes.UserId, "58eb2690-5266-4cbb-ab46-1f4a211ce9c0"),
        ]);
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
        DfeSignInClaimExtensions.TryGetDsiUserId(null!, out _);
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
        var identity = new ClaimsIdentity([
            new(DfeSignInClaimTypes.UserId, "58eb2690-5266-4cbb-ab46-1f4a211ce9c0"),
        ]);
        var user = new ClaimsPrincipal(identity);

        bool result = user.TryGetDsiUserId(out _);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TryGetDsiUserId_OutputsClaim_WhenClaimDoesExist()
    {
        var identity = new ClaimsIdentity([
            new(DfeSignInClaimTypes.UserId, "58eb2690-5266-4cbb-ab46-1f4a211ce9c0"),
        ]);
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
        DfeSignInClaimExtensions.GetDsiOrganisation(null!);
    }

    [TestMethod]
    public void GetDsiOrganisation_ReturnsNull_WhenClaimDoesNotExist()
    {
        var user = new ClaimsPrincipal();

        var result = user.GetDsiOrganisation();

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetDsiOrganisation_ReturnsClaimAsObjectWithId_WhenClaimDoesExist()
    {
        var identity = new ClaimsIdentity([
            new(DfeSignInClaimTypes.Organisation, /*lang=json,strict*/ """
                {
                    "id": "4db99a83-60ac-4e3f-b87f-81d384f673e7"
                }
            """, JsonClaimValueTypes.Json),
        ]);
        var user = new ClaimsPrincipal(identity);

        var result = user.GetDsiOrganisation();

        var expectedResult = new OrganisationClaim {
            Id = new Guid("4db99a83-60ac-4e3f-b87f-81d384f673e7"),
        };
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    public void GetDsiOrganisation_ReturnsClaimAsObjectWithMultipleProperties_WhenClaimDoesExist()
    {
        var identity = new ClaimsIdentity([
            new(DfeSignInClaimTypes.Organisation, /*lang=json,strict*/ """
                {
                    "id": "4db99a83-60ac-4e3f-b87f-81d384f673e7",
                    "name": "Example organisation name"
                }
            """, JsonClaimValueTypes.Json),
        ]);
        var user = new ClaimsPrincipal(identity);

        var result = user.GetDsiOrganisation();

        var expectedResult = new OrganisationClaim {
            Id = new Guid("4db99a83-60ac-4e3f-b87f-81d384f673e7"),
            Name = "Example organisation name",
        };
        Assert.AreEqual(expectedResult, result);
    }

    #endregion
}
