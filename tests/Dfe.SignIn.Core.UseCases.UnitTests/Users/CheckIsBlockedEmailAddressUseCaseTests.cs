using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.UseCases.Users;
using Microsoft.Extensions.Options;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class CheckIsBlockedEmailAddressUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            CheckIsBlockedEmailAddressRequest,
            CheckIsBlockedEmailAddressUseCase
        >();
    }

    private static void SetupFakeOptions(AutoMocker autoMocker)
    {
        autoMocker.GetMock<IOptionsMonitor<BlockedEmailAddressOptions>>()
            .Setup(x => x.CurrentValue)
            .Returns(new BlockedEmailAddressOptions {
                BlockedDomains = [
                    "blocked.com",
                    "blocked.other.com",
                ],
                BlockedNames = [
                    "admin",
                    "staff",
                    "pat",
                ],
            });
    }

    [TestMethod]
    [DataRow("alex.hunter@example.com")]
    [DataRow("xadmin@example.com")]
    [DataRow("staffmember@example.com")]
    [DataRow("patrick@example.com")]
    [DataRow("peter@example.com")]
    public async Task ReturnsExpectedResponse_WhenEmailAddressIsPermitted(string emailAddress)
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);

        var interactor = autoMocker.CreateInstance<CheckIsBlockedEmailAddressUseCase>();

        var response = await interactor.InvokeAsync(new CheckIsBlockedEmailAddressRequest {
            EmailAddress = emailAddress,
        }, CancellationToken.None);

        Assert.IsFalse(response.IsBlocked);
    }

    [TestMethod]
    [DataRow("admin@example.com")]
    [DataRow("admin123@example.com")]
    [DataRow("admin.test@example.com")]
    [DataRow("admin_test@example.com")]
    [DataRow("ADMIN@example.com")]
    [DataRow("staff@example.com")]
    [DataRow("STAFF@example.com")]
    [DataRow("pat@example.com")]
    [DataRow("pat123@example.com")]
    [DataRow("pat.test@example.com")]
    [DataRow("alex.hunter@blocked.com")]
    [DataRow("alex.hunter@BLOCKED.COM")]
    [DataRow("alex.hunter@blocked.other.com")]
    [DataRow("admin@blocked.com")]
    public async Task ReturnsExpectedResponse_WhenEmailAddressIsBlocked(string emailAddress)
    {
        var autoMocker = new AutoMocker();
        SetupFakeOptions(autoMocker);

        var interactor = autoMocker.CreateInstance<CheckIsBlockedEmailAddressUseCase>();

        var response = await interactor.InvokeAsync(new CheckIsBlockedEmailAddressRequest {
            EmailAddress = emailAddress,
        }, CancellationToken.None);

        Assert.IsTrue(response.IsBlocked);
    }
}
