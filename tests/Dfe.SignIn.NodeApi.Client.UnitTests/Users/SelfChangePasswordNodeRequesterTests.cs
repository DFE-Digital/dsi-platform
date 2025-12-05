using System.Net;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Interfaces.Graph;
using Dfe.SignIn.NodeApi.Client.Users;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class SelfChangePasswordNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            SelfChangePasswordRequest,
            SelfChangePasswordNodeRequester
        >();
    }

    private static SelfChangePasswordNodeRequester CreateSelfChangePasswordNodeRequester(
        AutoMocker autoMocker,
        Dictionary<string, MappedResponse> responseMappings)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        autoMocker.MockResponse(
            new GetUserProfileRequest {
                UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
            },
            new GetUserProfileResponse {
                IsEntra = false,
                IsInternalUser = false,
                GivenName = "Alex",
                Surname = "Cooper",
                EmailAddress = "alex.cooper@example.com",
            }
        );

        return new SelfChangePasswordNodeRequester(
            directoriesClient,
            autoMocker.Get<IInteractionDispatcher>(),
            autoMocker.Get<IGraphApiChangeUserPassword>()
        );
    }

    #region Entra user

    [TestMethod]
    public async Task EntraUser_ChangePasswordWithGraphApi_WhenGraphApiTokenProvided()
    {
        var autoMocker = new AutoMocker();
        var interactor = CreateSelfChangePasswordNodeRequester(autoMocker, []);

        var interactionContext = new InteractionContext<SelfChangePasswordRequest>(new() {
            UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
            CurrentPassword = "currentPass123",
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123",
            GraphAccessToken = new() {
                Token = "FakeToken",
                ExpiresOn = new DateTimeOffset(2025, 11, 10, 11, 53, 17, TimeSpan.Zero),
            },
        });

        await interactor.InvokeAsync(interactionContext);

        autoMocker.Verify<IGraphApiChangeUserPassword>(x =>
            x.ChangePassword(
                It.Is<InteractionContext<SelfChangePasswordRequest>>(context
                    => ReferenceEquals(context, interactionContext))
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task EntraUser_WritesToAudit_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        var interactor = CreateSelfChangePasswordNodeRequester(autoMocker, []);

        await interactor.InvokeAsync(new SelfChangePasswordRequest {
            UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
            CurrentPassword = "currentPass123",
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123",
            GraphAccessToken = new() {
                Token = "FakeToken",
                ExpiresOn = new DateTimeOffset(2025, 11, 10, 11, 53, 17, TimeSpan.Zero),
            },
        });

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangePassword, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Successfully changed password", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"), capturedWriteToAudit.UserId);
        Assert.IsFalse(capturedWriteToAudit.WasFailure);
    }

    [TestMethod]
    public async Task EntraUser_WritesToAudit_WhenFailed()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        autoMocker.GetMock<IGraphApiChangeUserPassword>()
            .Setup(x => x.ChangePassword(
                It.IsAny<InteractionContext<SelfChangePasswordRequest>>())
            )
            .Throws(new InvalidOperationException());

        var interactor = CreateSelfChangePasswordNodeRequester(autoMocker, []);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => interactor.InvokeAsync(new SelfChangePasswordRequest {
                UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
                CurrentPassword = "currentPass123",
                NewPassword = "NewPass123",
                ConfirmNewPassword = "NewPass123",
                GraphAccessToken = new() {
                    Token = "FakeToken",
                    ExpiresOn = new DateTimeOffset(2025, 11, 10, 11, 53, 17, TimeSpan.Zero),
                },
            }));

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangePassword, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Failed changed password", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"), capturedWriteToAudit.UserId);
        Assert.IsTrue(capturedWriteToAudit.WasFailure);
    }

    [TestMethod]
    public async Task EntraUser_ReturnsResponse()
    {
        var interactor = CreateSelfChangePasswordNodeRequester(new AutoMocker(), []);

        var response = await interactor.InvokeAsync(new SelfChangePasswordRequest {
            UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
            CurrentPassword = "currentPass123",
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123",
            GraphAccessToken = new() {
                Token = "FakeToken",
                ExpiresOn = new DateTimeOffset(2025, 11, 10, 11, 53, 17, TimeSpan.Zero),
            },
        });

        Assert.IsNotNull(response);
    }

    #endregion

    #region Non-Entra user

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(POST) http://directories.localhost/users/authenticate"] =
                new MappedResponse(HttpStatusCode.OK),
            ["(POST) http://directories.localhost/users/97c1d42e-88fd-4645-b110-84ccaac347a3/changepassword"] =
                new MappedResponse(HttpStatusCode.OK),
        };
    }

    [TestMethod]
    public async Task NonEntraUser_DoesNotChangePasswordWithGraphApi()
    {
        var autoMocker = new AutoMocker();
        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateSelfChangePasswordNodeRequester(autoMocker, mappings);

        await interactor.InvokeAsync(new SelfChangePasswordRequest {
            UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
            CurrentPassword = "currentPass123",
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123",
        });

        autoMocker.Verify<IGraphApiChangeUserPassword>(x =>
            x.ChangePassword(
                It.IsAny<InteractionContext<SelfChangePasswordRequest>>()
            ),
            Times.Never
        );
    }

    [TestMethod]
    public async Task NonEntraUser_MakesRequestToChangePassword()
    {
        var autoMocker = new AutoMocker();
        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateSelfChangePasswordNodeRequester(autoMocker, mappings);

        await interactor.InvokeAsync(new SelfChangePasswordRequest {
            UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
            CurrentPassword = "currentPass123",
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123",
        });

        var mapping = mappings["(POST) http://directories.localhost/users/97c1d42e-88fd-4645-b110-84ccaac347a3/changepassword"];
        Assert.HasCount(1, mapping.Invocations);

        var body = JsonSerializer.Deserialize<JsonElement>(mapping.Invocations[0].Body!);
        Assert.AreEqual("NewPass123", body.GetProperty("password").GetString());
    }

    [TestMethod]
    public async Task NonEntraUser_WritesToAudit_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateSelfChangePasswordNodeRequester(autoMocker, mappings);

        await interactor.InvokeAsync(new SelfChangePasswordRequest {
            UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
            CurrentPassword = "currentPass123",
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123",
        });

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangePassword, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Successfully changed password", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"), capturedWriteToAudit.UserId);
        Assert.IsFalse(capturedWriteToAudit.WasFailure);
    }

    [TestMethod]
    public async Task NonEntraUser_WritesToAudit_WhenFailed()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(req => capturedWriteToAudit = req);

        var interactor = CreateSelfChangePasswordNodeRequester(autoMocker, new() {
            ["(POST) http://directories.localhost/users/authenticate"] =
                new MappedResponse(HttpStatusCode.InternalServerError)
        });

        await Assert.ThrowsAsync<Exception>(()
            => interactor.InvokeAsync(new SelfChangePasswordRequest {
                UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
                CurrentPassword = "currentPass123",
                NewPassword = "NewPass123",
                ConfirmNewPassword = "NewPass123",
            }));

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangePassword, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Failed changed password", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"), capturedWriteToAudit.UserId);
        Assert.IsTrue(capturedWriteToAudit.WasFailure);
    }

    [TestMethod]
    public async Task NonEntraUser_ReturnsResponse()
    {
        var mappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateSelfChangePasswordNodeRequester(new AutoMocker(), mappings);

        var response = await interactor.InvokeAsync(new SelfChangePasswordRequest {
            UserId = new Guid("97c1d42e-88fd-4645-b110-84ccaac347a3"),
            CurrentPassword = "currentPass123",
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123",
        });

        Assert.IsNotNull(response);
    }

    #endregion
}
