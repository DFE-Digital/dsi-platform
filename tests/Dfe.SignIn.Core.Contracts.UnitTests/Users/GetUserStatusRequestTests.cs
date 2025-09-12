using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.Contracts.UnitTests.Users;

[TestClass]
public sealed class GetUserStatusRequestTests
{
    [TestMethod]
    public void ValidationFails_WhenMultipleOptionsAreSpecified()
    {
        var results = ValidationHelpers.ValidateAndExpectFailure(
            new GetUserStatusRequest {
                EntraUserId = new Guid("4fb8e842-1710-4949-9bb8-7ecf4b862f6a"),
                EmailAddress = "jo.davison@example.com",
            }
        );

        Assert.HasCount(1, results);
        Assert.AreEqual("Exactly one option must be specified.", results[0].ErrorMessage);
        Assert.HasCount(2, results[0].MemberNames);
    }

    [TestMethod]
    public void ValidationFails_WhenNoOptionsAreSpecified()
    {
        var results = ValidationHelpers.ValidateAndExpectFailure(
            new GetUserStatusRequest()
        );

        Assert.HasCount(1, results);
        Assert.AreEqual("Exactly one option must be specified.", results[0].ErrorMessage);
        Assert.HasCount(2, results[0].MemberNames);
    }
}
