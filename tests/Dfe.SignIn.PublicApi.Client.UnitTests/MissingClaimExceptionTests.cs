namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class MissingClaimExceptionTests
{
    #region Property: ClaimType

    [TestMethod]
    public void ClaimType_HasInitializedValue()
    {
        var exception = new MissingClaimException(null, DsiClaimTypes.UserId);

        Assert.AreEqual(DsiClaimTypes.UserId, exception.ClaimType);
    }

    #endregion
}
