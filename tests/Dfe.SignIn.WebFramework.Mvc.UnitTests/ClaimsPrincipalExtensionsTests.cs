using System.Security.Claims;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests;

[TestClass]
public sealed class ClaimsPrincipalExtensionsTests
{
    #region TryGetUserId(ClaimsPrincipal, out Guid?)

    [TestMethod]
    public void TryGetUserId_Throws_WhenPrincipalArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ClaimsPrincipalExtensions.TryGetUserId(null!, out _));
    }

    [TestMethod]
    public void TryGetUserId_ReturnsTrue_WhenClaimIsPresent()
    {
        var principal = new ClaimsPrincipal([
            new([
                new(ClaimTypes.NameIdentifier, "286101e9-a2dd-4894-bb3b-aefa8ea60ecd")
            ])
        ]);

        bool result = ClaimsPrincipalExtensions.TryGetUserId(principal, out var userId);
        Assert.IsTrue(result);
        Assert.AreEqual(Guid.Parse("286101e9-a2dd-4894-bb3b-aefa8ea60ecd"), userId);
    }

    [TestMethod]
    public void TryGetUserId_ReturnsFalse_WhenClaimIsNotPresent()
    {
        var principal = new ClaimsPrincipal();

        bool result = ClaimsPrincipalExtensions.TryGetUserId(principal, out var userId);
        Assert.IsFalse(result);
    }

    #endregion

    #region GetUserId(ClaimsPrincipal)

    [TestMethod]
    public void GetUserId_Throws_WhenPrincipalArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ClaimsPrincipalExtensions.GetUserId(null!));
    }

    [TestMethod]
    public void GetUserId_Throws_WhenMissingUserId()
    {
        var principal = new ClaimsPrincipal();

        var exception = Assert.ThrowsExactly<InvalidOperationException>(()
            => ClaimsPrincipalExtensions.GetUserId(principal));
        Assert.AreEqual("Missing user ID.", exception.Message);
    }

    [TestMethod]
    public void GetUserId_ReturnsUserId()
    {
        var principal = new ClaimsPrincipal([
            new([
                new(ClaimTypes.NameIdentifier, "286101e9-a2dd-4894-bb3b-aefa8ea60ecd")
            ])
        ]);

        var userId = ClaimsPrincipalExtensions.GetUserId(principal);

        Assert.AreEqual(Guid.Parse("286101e9-a2dd-4894-bb3b-aefa8ea60ecd"), userId);
    }

    #endregion
}
