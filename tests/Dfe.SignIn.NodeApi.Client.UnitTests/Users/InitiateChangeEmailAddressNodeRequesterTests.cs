using System.Net;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;
using Microsoft.Extensions.DependencyInjection;
using Moq.AutoMock;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class InitiateChangeEmailAddressNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            InitiateChangeEmailAddressRequest,
            InitiateChangeEmailAddressNodeRequester
        >();
    }

    private static InitiateChangeEmailAddressNodeRequester CreateInitiateChangeEmailAddressNodeRequester(
        AutoMocker autoMocker,
        Dictionary<string, MappedResponse> responseMappings)
    {
        autoMocker.MockResponse(
            new GetUserStatusRequest { EmailAddress = "alex.cooper@example.com" },
            new GetUserStatusResponse {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                AccountStatus = AccountStatus.Active,
                UserExists = true,
            }
        );

        autoMocker.MockResponse(
            new GetUserProfileRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            },
            new GetUserProfileResponse {
                IsEntra = false,
                IsInternalUser = false,
                FirstName = "Alex",
                LastName = "Cooper",
                EmailAddress = "alex.cooper@example.com",
            }
        );

        autoMocker.MockResponse(
            new GetUserStatusRequest { EmailAddress = "alex.other@example.com" },
            new GetUserStatusResponse {
                UserId = new Guid("cbaebfb6-04c4-436d-8acf-77c4281e3b06"),
                AccountStatus = AccountStatus.Active,
                UserExists = true,
            }
        );

        autoMocker.MockResponse(
            new GetUserStatusRequest { EmailAddress = "alex.new@example.com" },
            new GetUserStatusResponse { UserExists = false }
        );

        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new InitiateChangeEmailAddressNodeRequester(
            directoriesClient,
            autoMocker.Get<IInteractionDispatcher>(),
            autoMocker.Get<IInteractionLimiter>()
        );
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(DELETE) http://directories.localhost/usercodes/51a50a75-e4fa-4b6e-9c72-581538ee5258/changeemail"] =
                new MappedResponse(HttpStatusCode.OK),
            ["(PUT) http://directories.localhost/usercodes/upsert"] =
                new MappedResponse(HttpStatusCode.OK),
        };
    }

    [TestMethod]
    public async Task Throws_WhenUserAttemptsToUseTheirCurrentEmailAddress()
    {
        var interactor = CreateInitiateChangeEmailAddressNodeRequester(new AutoMocker(), []);

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                ClientId = "test",
                IsSelfInvoked = true,
                NewEmailAddress = "alex.cooper@example.com",
            }));

        InteractionAssert.HasValidationError(exception,
            "Input an email address that is different from your current email address",
            nameof(InitiateChangeEmailAddressRequest.NewEmailAddress)
        );
    }

    [TestMethod]
    public async Task Throws_WhenNewEmailAddressAlreadyTakenByAnotherUser()
    {
        var autoMocker = new AutoMocker();

        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateInitiateChangeEmailAddressNodeRequester(autoMocker, mappings);

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                ClientId = "test",
                IsSelfInvoked = true,
                NewEmailAddress = "alex.other@example.com",
            }));

        InteractionAssert.HasValidationError(exception,
            "Please enter a valid new email address",
            nameof(InitiateChangeEmailAddressRequest.NewEmailAddress)
        );
    }

    [TestMethod]
    public async Task WritesToAudit_WhenNewEmailAddressAlreadyTakenByAnotherUser()
    {
        var autoMocker = new AutoMocker();

        var capturedWriteToAudit = new List<WriteToAuditRequest>();
        autoMocker.CaptureRequest<WriteToAuditRequest>(capturedWriteToAudit.Add);

        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateInitiateChangeEmailAddressNodeRequester(autoMocker, mappings);

        await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                ClientId = "test",
                IsSelfInvoked = true,
                NewEmailAddress = "alex.other@example.com",
            }));

        var writeToAudit = capturedWriteToAudit.FirstOrDefault(req
            => req.EventName == AuditChangeEmailEventNames.RequestedExistingEmail);
        Assert.IsNotNull(writeToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeEmail, writeToAudit.EventCategory);
        Assert.AreEqual("Request to change email from alex.cooper@example.com to existing user alex.other@example.com", writeToAudit.Message);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), writeToAudit.UserId);
    }

    [TestMethod]
    public async Task AppliesActionLimiter()
    {
        var autoMocker = new AutoMocker();

        var request = new InitiateChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ClientId = "test",
            IsSelfInvoked = true,
            NewEmailAddress = "alex.new@example.com",
        };

        autoMocker.MockLimiter(request, reject: true);

        var interactor = CreateInitiateChangeEmailAddressNodeRequester(autoMocker, []);

        await Assert.ThrowsExactlyAsync<InteractionRejectedByLimiterException>(()
            => interactor.InvokeAsync(request));
    }

    [TestMethod]
    public async Task WritesToAuditBeforeInitiatingChange()
    {
        var autoMocker = new AutoMocker();

        var capturedWriteToAudit = new List<WriteToAuditRequest>();
        autoMocker.CaptureRequest<WriteToAuditRequest>(capturedWriteToAudit.Add);

        autoMocker.MockLimiter<InitiateChangeEmailAddressRequest>(reject: false);

        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateInitiateChangeEmailAddressNodeRequester(autoMocker, mappings);

        await interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ClientId = "test",
            IsSelfInvoked = true,
            NewEmailAddress = "alex.new@example.com",
        });

        var writeToAudit = capturedWriteToAudit.FirstOrDefault(req
            => req.EventName == AuditChangeEmailEventNames.RequestToChangeEmail);
        Assert.IsNotNull(writeToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeEmail, writeToAudit.EventCategory);
        Assert.AreEqual("Request to change email from alex.cooper@example.com to alex.new@example.com", writeToAudit.Message);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), writeToAudit.UserId);
    }

    [TestMethod]
    public async Task MakesRequestToDeleteAnyExistingVerificationCode()
    {
        var autoMocker = new AutoMocker();
        autoMocker.MockLimiter<InitiateChangeEmailAddressRequest>(reject: false);

        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateInitiateChangeEmailAddressNodeRequester(autoMocker, mappings);

        await interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ClientId = "test",
            IsSelfInvoked = true,
            NewEmailAddress = "alex.new@example.com",
        });

        var mapping = mappings["(DELETE) http://directories.localhost/usercodes/51a50a75-e4fa-4b6e-9c72-581538ee5258/changeemail"];
        Assert.HasCount(1, mapping.Invocations);
    }

    [TestMethod]
    public async Task MakesRequestToCreateNewVerificationCode()
    {
        var autoMocker = new AutoMocker();
        autoMocker.MockLimiter<InitiateChangeEmailAddressRequest>(reject: false);

        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateInitiateChangeEmailAddressNodeRequester(autoMocker, mappings);

        await interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ClientId = "test",
            IsSelfInvoked = true,
            NewEmailAddress = "alex.new@example.com",
        });

        var mapping = mappings["(PUT) http://directories.localhost/usercodes/upsert"];
        Assert.HasCount(1, mapping.Invocations);

        var body = JsonSerializer.Deserialize<JsonElement>(mapping.Invocations[0].Body!);
        Assert.AreEqual("51a50a75-e4fa-4b6e-9c72-581538ee5258", body.GetProperty("uid").GetString());
        Assert.AreEqual("test", body.GetProperty("clientId").GetString());
        Assert.AreEqual("n/a", body.GetProperty("redirectUri").GetString());
        Assert.AreEqual("changeemail", body.GetProperty("codeType").GetString());
        Assert.AreEqual("alex.new@example.com", body.GetProperty("email").GetString());
        Assert.IsTrue(body.GetProperty("selfInvoked").GetBoolean());
    }

    [TestMethod]
    public async Task ReturnsResponse()
    {
        var autoMocker = new AutoMocker();
        autoMocker.MockLimiter<InitiateChangeEmailAddressRequest>(reject: false);

        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateInitiateChangeEmailAddressNodeRequester(autoMocker, mappings);

        var response = await interactor.InvokeAsync(new InitiateChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ClientId = "test",
            IsSelfInvoked = true,
            NewEmailAddress = "alex.new@example.com",
        });

        Assert.IsNotNull(response);
    }
}
