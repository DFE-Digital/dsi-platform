using System.Linq.Expressions;
using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.Tests.SelectOrganisation;

[TestClass]
public sealed class CreateSelectOrganisationSession_UseCaseTests
{
    private static readonly CreateSelectOrganisationSessionRequest FakeRequest = new() {
        CallbackUrl = new Uri("https://example.localhost/callback"),
        ClientId = "test-client",
        UserId = new Guid("d4176ff4-ff7e-4e3b-86ad-2aa9890b26c4"),
    };

    #region InvokeAsync(CreateSelectOrganisationSessionRequest)

    private static async Task<(CreateSelectOrganisationSessionResponse, string?)> InvokeCaptureSessionKey(
        AutoMocker? autoMocker = null)
    {
        if (autoMocker == null) {
            autoMocker = new AutoMocker();
            autoMocker.Use<IOptions<SelectOrganisationOptions>>(new SelectOrganisationOptions());
        }

        string? capturedSessionKey = null;
        autoMocker.GetMock<ISelectOrganisationSessionRepository>()
            .Setup(x =>
                x.StoreAsync(It.IsAny<string>(), It.IsAny<SelectOrganisationSessionData>())
            )
            .Callback<string, SelectOrganisationSessionData>((sessionKey, _sessionData) => {
                capturedSessionKey = sessionKey;
            });

        var useCase = autoMocker.CreateInstance<CreateSelectOrganisationSession_UseCase>();

        var response = await useCase.InvokeAsync(FakeRequest);

        return (response, capturedSessionKey);
    }

    private static async Task VerifyInvokeAsyncSession(
        Expression<Func<SelectOrganisationSessionData, bool>> match,
        AutoMocker? autoMocker = null)
    {
        if (autoMocker == null) {
            autoMocker = new AutoMocker();
            autoMocker.Use<IOptions<SelectOrganisationOptions>>(new SelectOrganisationOptions());
        }

        var useCase = autoMocker.CreateInstance<CreateSelectOrganisationSession_UseCase>();

        await useCase.InvokeAsync(FakeRequest);

        autoMocker.Verify<ISelectOrganisationSessionRepository>(
            x => x.StoreAsync(
                It.IsAny<string>(),
                It.Is(match)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task InvokeAsync_StoresSessionInRepositoryWithUniqueKey()
    {
        var (response1, capturedSessionKey1) = await InvokeCaptureSessionKey();
        var (response2, capturedSessionKey2) = await InvokeCaptureSessionKey();

        Assert.AreNotEqual(capturedSessionKey1, capturedSessionKey2);
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedCallbackUrl()
    {
        await VerifyInvokeAsyncSession(session =>
            session.CallbackUrl == FakeRequest.CallbackUrl
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedClientId()
    {
        await VerifyInvokeAsyncSession(session =>
            session.ClientId == FakeRequest.ClientId
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedUserId()
    {
        await VerifyInvokeAsyncSession(session =>
            session.UserId == FakeRequest.UserId
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedPrompt()
    {
        await VerifyInvokeAsyncSession(session =>
            session.Prompt == FakeRequest.Prompt
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedOrganisations()
    {
        var organisationOptions = new SelectOrganisationOption[] { };

        // TODO: Inject fake organisation options...

        await VerifyInvokeAsyncSession(session =>
            session.OrganisationOptions.Intersect(organisationOptions)
                .Count() == organisationOptions.Length
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedCreationTime()
    {
        await VerifyInvokeAsyncSession(session =>
            // Was created within the past 5 seconds?
            (DateTime.UtcNow - session.Created) < new TimeSpan(0, 0, 5)
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedSessionTimeout()
    {
        var autoMocker = new AutoMocker();
        autoMocker.Use<IOptions<SelectOrganisationOptions>>(new SelectOrganisationOptions {
            SessionTimeoutInMinutes = 10,
        });

        await VerifyInvokeAsyncSession(session =>
            (session.Expires - session.Created) == new TimeSpan(0, 10, 0),
            autoMocker
        );
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedUrl()
    {
        var autoMocker = new AutoMocker();
        autoMocker.Use<IOptions<SelectOrganisationOptions>>(new SelectOrganisationOptions {
            SelectOrganisationBaseAddress = new Uri("https://select-organisation.localhost"),
        });

        var (response, capturedSessionKey) = await InvokeCaptureSessionKey(autoMocker);

        Assert.AreEqual(
            $"https://select-organisation.localhost/test-client/{capturedSessionKey}",
            response.Url.ToString()
        );
    }

    #endregion
}
