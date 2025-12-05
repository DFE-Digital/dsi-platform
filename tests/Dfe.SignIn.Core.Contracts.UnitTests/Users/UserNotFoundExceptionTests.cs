using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.Contracts.UnitTests.Users;

[TestClass]
public sealed class UserNotFoundExceptionTests
{
    #region Property: UserId

    [TestMethod]
    public void UserId_IsNull_whenNotInitialized()
    {
        var exception = new UserNotFoundException();

        Assert.IsNull(exception.UserId);
    }

    [TestMethod]
    public void UserId_HasInitializedValue()
    {
        var exception = UserNotFoundException.FromUserId(new Guid("51c5c002-8fe1-4a57-814e-8c15294b3e2e"));

        Assert.AreEqual(new Guid("51c5c002-8fe1-4a57-814e-8c15294b3e2e"), exception.UserId);
    }

    #endregion
}
