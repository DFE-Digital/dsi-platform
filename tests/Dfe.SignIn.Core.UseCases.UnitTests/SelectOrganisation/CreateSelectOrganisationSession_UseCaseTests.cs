using System.Linq.Expressions;
using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.SelectOrganisation;

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
        CreateSelectOrganisationSessionRequest request,
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

        var response = await useCase.InvokeAsync(request);

        return (response, capturedSessionKey);
    }

    private static async Task VerifyInvokeAsyncSession(
        CreateSelectOrganisationSessionRequest request,
        Expression<Func<SelectOrganisationSessionData, bool>> match,
        AutoMocker? autoMocker = null)
    {
        if (autoMocker == null) {
            autoMocker = new AutoMocker();
            autoMocker.Use<IOptions<SelectOrganisationOptions>>(new SelectOrganisationOptions());
        }

        var useCase = autoMocker.CreateInstance<CreateSelectOrganisationSession_UseCase>();

        await useCase.InvokeAsync(request);

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
        var (response1, capturedSessionKey1) = await InvokeCaptureSessionKey(FakeRequest);
        var (response2, capturedSessionKey2) = await InvokeCaptureSessionKey(FakeRequest);

        Assert.AreNotEqual(capturedSessionKey1, capturedSessionKey2);
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedCallbackUrl()
    {
        await VerifyInvokeAsyncSession(FakeRequest, session =>
            session.CallbackUrl == FakeRequest.CallbackUrl
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedClientId()
    {
        await VerifyInvokeAsyncSession(FakeRequest, session =>
            session.ClientId == FakeRequest.ClientId
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedUserId()
    {
        await VerifyInvokeAsyncSession(FakeRequest, session =>
            session.UserId == FakeRequest.UserId
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedPrompt_WhenPromptIsNotSpecified()
    {
        await VerifyInvokeAsyncSession(FakeRequest, session =>
            session.Prompt.Heading == "Which organisation would you like to use?" &&
            session.Prompt.Hint == "You are associated with more than one organisation. Select one option."
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedPrompt_WhenPromptIsSpecified()
    {
        var request = FakeRequest with {
            Prompt = new() {
                Heading = "Which organisation would you like to contact?",
                Hint = "You are associated with multiple organisations. Select one option.",
            },
        };
        await VerifyInvokeAsyncSession(request, session =>
            session.Prompt.Heading == request.Prompt.Heading &&
            session.Prompt.Hint == request.Prompt.Hint
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedPrompt_WhenHeadingIsSpecified()
    {
        var request = FakeRequest with {
            Prompt = new() {
                Heading = "Which organisation would you like to contact?",
            },
        };
        await VerifyInvokeAsyncSession(request, session =>
            session.Prompt.Heading == request.Prompt.Heading &&
            session.Prompt.Hint == "Select one option."
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedOrganisations()
    {
        var organisationOptions = new SelectOrganisationOption[] { };

        // TODO: Inject fake organisation options...

        await VerifyInvokeAsyncSession(FakeRequest, session =>
            session.OrganisationOptions.Intersect(organisationOptions)
                .Count() == organisationOptions.Length
        );
    }

    [TestMethod]
    public async Task InvokeAsync_SessionHasExpectedCreationTime()
    {
        await VerifyInvokeAsyncSession(FakeRequest, session =>
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

        await VerifyInvokeAsyncSession(FakeRequest, session =>
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

        var (response, capturedSessionKey) = await InvokeCaptureSessionKey(FakeRequest, autoMocker);

        Assert.AreEqual(
            $"https://select-organisation.localhost/test-client/{capturedSessionKey}",
            response.Url.ToString()
        );
    }

    #endregion
}
