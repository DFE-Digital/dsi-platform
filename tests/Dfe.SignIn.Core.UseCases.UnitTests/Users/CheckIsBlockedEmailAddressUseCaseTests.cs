using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.UseCases.Users;
using Microsoft.Extensions.Options;
using Moq;
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
                ],
            });
    }

    [TestMethod]
    [DataRow("alex.hunter@example.com")]
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
    [DataRow("staff@example.com")]
    [DataRow("alex.hunter@blocked.com")]
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
