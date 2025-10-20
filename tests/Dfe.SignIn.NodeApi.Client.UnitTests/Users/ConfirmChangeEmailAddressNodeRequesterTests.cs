using System.Net;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class ConfirmChangeEmailAddressNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            ConfirmChangeEmailAddressRequest,
            ConfirmChangeEmailAddressNodeRequester
        >();
    }

    private static ConfirmChangeEmailAddressNodeRequester CreateConfirmChangeEmailAddressNodeRequester(
        AutoMocker autoMocker,
        Dictionary<string, MappedResponse> responseMappings)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        autoMocker.MockResponse(
            new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            },
            new GetPendingChangeEmailAddressResponse {
                PendingChangeEmailAddress = new() {
                    UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                    VerificationCode = "ABC1234",
                    NewEmailAddress = "alex.cooper@example.com",
                    ExpiryTimeUtc = new DateTime(2025, 11, 5, 11, 23, 17, DateTimeKind.Utc),
                    HasExpired = false,
                },
            }
        );

        return new ConfirmChangeEmailAddressNodeRequester(
            directoriesClient,
            autoMocker.Get<IInteractionDispatcher>()
        );
    }

    private static void SetupExpiredPendingChangeEmailRequest(AutoMocker autoMocker)
    {
        autoMocker.MockResponse(
            new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("1615a351-f7eb-45bc-83f3-c45d021eceb4"),
            },
            new GetPendingChangeEmailAddressResponse {
                PendingChangeEmailAddress = new() {
                    UserId = new Guid("1615a351-f7eb-45bc-83f3-c45d021eceb4"),
                    VerificationCode = "ABC1234",
                    NewEmailAddress = "alex.cooper@example.com",
                    ExpiryTimeUtc = new DateTime(2022, 11, 5, 11, 23, 17, DateTimeKind.Utc),
                    HasExpired = true,
                },
            }
        );
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.OK),
            ["(DELETE) http://directories.localhost/usercodes/51a50a75-e4fa-4b6e-9c72-581538ee5258/changeemail"] =
                new MappedResponse(HttpStatusCode.OK),
        };
    }

    [TestMethod]
    public async Task Throws_WhenNoPendingChangeEmail()
    {
        var autoMocker = new AutoMocker();
        autoMocker.MockResponse(
            new GetPendingChangeEmailAddressRequest {
                UserId = new Guid("1615a351-f7eb-45bc-83f3-c45d021eceb4"),
            },
            new GetPendingChangeEmailAddressResponse {
                PendingChangeEmailAddress = null,
            }
        );

        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(autoMocker, responseMappings);

        await Assert.ThrowsExactlyAsync<NoPendingChangeEmailException>(()
            => interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
                UserId = new Guid("1615a351-f7eb-45bc-83f3-c45d021eceb4"),
                VerificationCode = "ABC1234",
            }));
    }

    [TestMethod]
    public async Task Throws_WhenVerificationCodeWasIncorrect()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(new AutoMocker(), responseMappings);

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                VerificationCode = "DEF56789",
            }));

        InteractionAssert.HasValidationError(exception,
            "The verification code you entered is incorrect. Please check and try again.",
            nameof(ConfirmChangeEmailAddressRequest.VerificationCode)
        );
    }

    [TestMethod]
    public async Task WritesToAudit_WhenVerificationCodeWasIncorrect()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(request => capturedWriteToAudit = request);

        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(autoMocker, responseMappings);

        await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                VerificationCode = "DEF56789",
            }));

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeEmail, capturedWriteToAudit.EventCategory);
        Assert.AreEqual(AuditChangeEmailEventNames.EmailChangeFailed, capturedWriteToAudit.EventName);
        Assert.AreEqual("Failed changed email to alex.cooper@example.com - invalid code", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), capturedWriteToAudit.UserId);
        Assert.IsTrue(capturedWriteToAudit.WasFailure);
    }

    [TestMethod]
    public async Task Throws_WhenPendingChangeEmailHasExpired()
    {
        var autoMocker = new AutoMocker();
        SetupExpiredPendingChangeEmailRequest(autoMocker);

        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(autoMocker, responseMappings);

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
                UserId = new Guid("1615a351-f7eb-45bc-83f3-c45d021eceb4"),
                VerificationCode = "ABC1234",
            }));

        InteractionAssert.HasValidationError(exception,
            "The verification code has expired",
            nameof(ConfirmChangeEmailAddressRequest.VerificationCode)
        );
    }

    [TestMethod]
    public async Task WritesToAudit_WhenPendingChangeEmailHasExpired()
    {
        var autoMocker = new AutoMocker();
        SetupExpiredPendingChangeEmailRequest(autoMocker);

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(request => capturedWriteToAudit = request);

        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(autoMocker, responseMappings);

        await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
                UserId = new Guid("1615a351-f7eb-45bc-83f3-c45d021eceb4"),
                VerificationCode = "DEF56789",
            }));

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeEmail, capturedWriteToAudit.EventCategory);
        Assert.AreEqual(AuditChangeEmailEventNames.EnteredExpiredCode, capturedWriteToAudit.EventName);
        Assert.AreEqual("Verification code ABC1234 expired at 05/11/2022 11:23:17", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("1615a351-f7eb-45bc-83f3-c45d021eceb4"), capturedWriteToAudit.UserId);
        Assert.IsTrue(capturedWriteToAudit.WasFailure);
    }

    [TestMethod]
    public async Task MakesExpectedRequestToPatchUser()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(new AutoMocker(), responseMappings);

        var response = await interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            VerificationCode = "ABC1234",
        });

        Assert.IsNotNull(response);

        var mapping = responseMappings["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"];
        Assert.HasCount(1, mapping.Invocations);

        var body = JsonSerializer.Deserialize<JsonElement>(mapping.Invocations[0].Body!);
        Assert.AreEqual("alex.cooper@example.com", body.GetProperty("email").GetString());
    }

    [TestMethod]
    public async Task Throws_WhenFailedToUpdateAuthenticationMethod()
    {
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(new AutoMocker(), new() {
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.InternalServerError, /*lang=json,strict*/ """
                {
                    "type": "ChangeEmailAddressAuthenticationMethodError"
                }
                """)
        });

        await Assert.ThrowsExactlyAsync<FailedToUpdateAuthenticationMethodException>(()
            => interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                VerificationCode = "ABC1234",
            }));
    }

    [TestMethod]
    public async Task WritesToAudit_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(request => capturedWriteToAudit = request);

        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(autoMocker, responseMappings);

        await interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            VerificationCode = "ABC1234",
        });

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeEmail, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Successfully changed email to alex.cooper@example.com", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), capturedWriteToAudit.UserId);
        Assert.IsFalse(capturedWriteToAudit.WasFailure);

        var customProperties = capturedWriteToAudit.CustomProperties.ToDictionary(pair => pair.Key, pair => pair.Value);
        var editedFields = customProperties["editedFields"] as dynamic[];
        Assert.IsNotNull(editedFields);

        var newEmailField = editedFields.First(field => field.name == "new_email");
        Assert.AreEqual("alex.cooper@example.com", newEmailField.newValue);
    }

    [TestMethod]
    public async Task DeletesVerificationCode_WhenSuccessful()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(new AutoMocker(), responseMappings);

        await interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            VerificationCode = "ABC1234",
        });

        var mapping = responseMappings["(DELETE) http://directories.localhost/usercodes/51a50a75-e4fa-4b6e-9c72-581538ee5258/changeemail"];
        Assert.HasCount(1, mapping.Invocations);
    }

    [TestMethod]
    public async Task Throws_WhenAnUnexpectedErrorOccurs()
    {
        var interactor = CreateConfirmChangeEmailAddressNodeRequester(new AutoMocker(), new() {
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.InternalServerError)
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                VerificationCode = "ABC1234",
            }));
    }

    [TestMethod]
    public async Task WritesToAudit_WhenAnUnexpectedErrorOccurs()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(request => capturedWriteToAudit = request);

        var interactor = CreateConfirmChangeEmailAddressNodeRequester(autoMocker, new() {
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.InternalServerError)
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new ConfirmChangeEmailAddressRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                VerificationCode = "ABC1234",
            }));

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeEmail, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Failed changed email to alex.cooper@example.com - Response status code does not indicate success: 500 (Internal Server Error).", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), capturedWriteToAudit.UserId);
        Assert.IsTrue(capturedWriteToAudit.WasFailure);
    }
}
