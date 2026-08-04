using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Fn.AuthExtensions.OnTokenIssuanceStart;
using Microsoft.AspNetCore.Mvc;
using Moq.AutoMock;

namespace Dfe.SignIn.Fn.AuthExtensions.UnitTests;

[TestClass]
public class TokenIssuanceStartHandlerTests
{
    private static readonly TokenIssuanceStartEvent FakeEvent = new() {
        Type = "microsoft.graph.authenticationEvent.tokenIssuanceStart",
        Data = new() {
            Type = "microsoft.graph.onTokenIssuanceStartCalloutData",
            TenantId = new Guid("2b01ae60-a8c3-4a99-9be3-a981e2861c35"),
            AuthenticationContext = new() {
                CorrelationId = "6d75195d-b584-429f-86cf-7a3046306f03",
                User = new() {
                    Id = new Guid("21892c65-88df-4268-b025-d06f51c52404"),
                    Mail = "jo.bradford@example.com",
                    DisplayName = "BRADFORD, Jo",
                    GivenName = "Jo",
                    Surname = "Bradford",
                    UserPrincipalName = "jo.bradford@example.com",
                    UserType = "Member",
                },
            },
        },
    };

    [TestMethod]
    [DataRow("", "Invalid event type ''.")]
    [DataRow("microsoft.graph.invalidEvent", "Invalid event type 'microsoft.graph.invalidEvent'.")]
    public async Task Throws_WhenGivenUnexpectedEventType(
        string eventType, string expectedMessage)
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Type = eventType,
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(()
            => handler.Run(fakeRequest));
        Assert.AreEqual(expectedMessage, exception.Message);
    }

    [TestMethod]
    [DataRow("", "Invalid callout data type ''.")]
    [DataRow("microsoft.graph.invalidCalloutDataType", "Invalid callout data type 'microsoft.graph.invalidCalloutDataType'.")]
    public async Task Throws_WhenGivenUnexpectedCalloutDataType(
        string calloutDataType, string expectedMessage)
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                Type = calloutDataType,
            },
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(()
            => handler.Run(fakeRequest));
        Assert.AreEqual(expectedMessage, exception.Message);
    }

    [TestMethod]
    public async Task Throws_WhenMailAttributeInvalid()
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                AuthenticationContext = FakeEvent.Data.AuthenticationContext with {
                    User = FakeEvent.Data.AuthenticationContext.User with {
                        Mail = "abc",
                    },
                },
            },
        });

        var exception = await Assert.ThrowsAsync<ValidationException>(()
            => handler.Run(fakeRequest));
        Assert.HasCount(1, exception.ValidationResult.MemberNames);
        Assert.Contains(nameof(UserContext.Mail), exception.ValidationResult.MemberNames);
    }

    [TestMethod]
    public async Task Throws_WhenGivenNameAttributeInvalid()
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                AuthenticationContext = FakeEvent.Data.AuthenticationContext with {
                    User = FakeEvent.Data.AuthenticationContext.User with {
                        GivenName = "",
                    },
                },
            },
        });

        var exception = await Assert.ThrowsAsync<ValidationException>(()
            => handler.Run(fakeRequest));
        Assert.HasCount(1, exception.ValidationResult.MemberNames);
        Assert.Contains(nameof(UserContext.GivenName), exception.ValidationResult.MemberNames);
    }

    [TestMethod]
    public async Task Throws_WhenSurnameAttributeInvalid()
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                AuthenticationContext = FakeEvent.Data.AuthenticationContext with {
                    User = FakeEvent.Data.AuthenticationContext.User with {
                        Surname = "",
                    },
                },
            },
        });

        var exception = await Assert.ThrowsAsync<ValidationException>(()
            => handler.Run(fakeRequest));
        Assert.HasCount(1, exception.ValidationResult.MemberNames);
        Assert.Contains(nameof(UserContext.Surname), exception.ValidationResult.MemberNames);
    }

    [TestMethod]
    public async Task AutomaticallyLinkEntraUserToDsi()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new AutoLinkEntraUserToDsiRequest {
                EntraUserId = new Guid("21892c65-88df-4268-b025-d06f51c52404"),
                EmailAddress = "jo.bradford@example.com",
                FirstName = "Jo",
                LastName = "Bradford",
            },
            new AutoLinkEntraUserToDsiResponse {
                UserId = new Guid("d5ba1f44-1400-4c98-b834-5d5ba5b98995"),
            }
        );

        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent);

        var result = await handler.Run(fakeRequest);

        var okResult = TypeAssert.IsType<OkObjectResult>(result);
        var response = TypeAssert.IsType<ResponseObject>(okResult.Value);
        var data = TypeAssert.IsType<TokenIssuanceStartEventResponseData>(response.Data);
        Assert.HasCount(1, data.Actions);
        var provideClaimsAction = TypeAssert.IsType<ProvideClaimsForTokenAction>(data.Actions[0]);
        Assert.AreEqual("d5ba1f44-1400-4c98-b834-5d5ba5b98995", provideClaimsAction.Claims[DsiClaimTypes.UserId]);
    }

    [TestMethod]
    public async Task AutomaticallyLinkEntraUserToDsi_TrimsUserAttributesBeforeDispatch()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new AutoLinkEntraUserToDsiRequest {
                EntraUserId = new Guid("21892c65-88df-4268-b025-d06f51c52404"),
                EmailAddress = "jo.bradford@example.com",
                FirstName = "Jo",
                LastName = "MAY - FINNEGAN",
            },
            new AutoLinkEntraUserToDsiResponse {
                UserId = new Guid("d5ba1f44-1400-4c98-b834-5d5ba5b98995"),
            }
        );

        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                AuthenticationContext = FakeEvent.Data.AuthenticationContext with {
                    User = FakeEvent.Data.AuthenticationContext.User with {
                        Mail = "jo.bradford@example.com",
                        GivenName = " Jo ",
                        Surname = " MAY - FINNEGAN ",
                    },
                },
            },
        });

        var result = await handler.Run(fakeRequest);

        var okResult = TypeAssert.IsType<OkObjectResult>(result);
        var response = TypeAssert.IsType<ResponseObject>(okResult.Value);
        var data = TypeAssert.IsType<TokenIssuanceStartEventResponseData>(response.Data);
        Assert.HasCount(1, data.Actions);
        var provideClaimsAction = TypeAssert.IsType<ProvideClaimsForTokenAction>(data.Actions[0]);
        Assert.AreEqual("d5ba1f44-1400-4c98-b834-5d5ba5b98995", provideClaimsAction.Claims[DsiClaimTypes.UserId]);
    }

    [TestMethod]
    public async Task WritesAuditAndRethrows_WhenUserCannotBeLinkedBecauseInactive()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<AutoLinkEntraUserToDsiRequest>(new CannotLinkInactiveUserException());

        var capturedWriteToAudit = new List<WriteToAuditRequest>();
        autoMocker.CaptureRequest<WriteToAuditRequest>(capturedWriteToAudit.Add);

        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent);

        await Assert.ThrowsAsync<CannotLinkInactiveUserException>(()
            => handler.Run(fakeRequest));

        Assert.HasCount(1, capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.Auth, capturedWriteToAudit[0].EventCategory);
        Assert.AreEqual(AuditAuthEventNames.LinkFailed, capturedWriteToAudit[0].EventName);
        Assert.IsTrue(capturedWriteToAudit[0].WasFailure);
        StringAssert.Contains(capturedWriteToAudit[0].Message, "permanent failure requiring account reactivation");
    }

    [TestMethod]
    public async Task WritesAuditAndRethrows_WhenUserCannotBeCreated()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<AutoLinkEntraUserToDsiRequest>(
            CannotCreateNewUserException.FromEmailAddress("jo.bradford@example.com"));

        var capturedWriteToAudit = new List<WriteToAuditRequest>();
        autoMocker.CaptureRequest<WriteToAuditRequest>(capturedWriteToAudit.Add);

        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent);

        await Assert.ThrowsAsync<CannotCreateNewUserException>(()
            => handler.Run(fakeRequest));

        Assert.HasCount(1, capturedWriteToAudit);
        Assert.AreEqual(AuditAuthEventNames.LinkFailed, capturedWriteToAudit[0].EventName);
        StringAssert.Contains(capturedWriteToAudit[0].Message, "may self-heal on the user's next sign-in");
    }

    [TestMethod]
    public async Task RethrowsOriginalException_WhenAuditDispatchItselfThrows()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockThrows<AutoLinkEntraUserToDsiRequest>(new CannotLinkInactiveUserException());
        autoMocker.MockThrows<WriteToAuditRequest>(new InvalidOperationException("Audit service unavailable."));

        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent);

        // The original, diagnosable exception must still propagate even though writing
        // the audit entry itself failed — an audit-infrastructure error must never mask it.
        await Assert.ThrowsAsync<CannotLinkInactiveUserException>(()
            => handler.Run(fakeRequest));
    }

    [TestMethod]
    public async Task DoesNotWriteAudit_WhenLinkingSucceeds()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new AutoLinkEntraUserToDsiRequest {
                EntraUserId = new Guid("21892c65-88df-4268-b025-d06f51c52404"),
                EmailAddress = "jo.bradford@example.com",
                FirstName = "Jo",
                LastName = "Bradford",
            },
            new AutoLinkEntraUserToDsiResponse {
                UserId = new Guid("d5ba1f44-1400-4c98-b834-5d5ba5b98995"),
            }
        );

        var capturedWriteToAudit = new List<WriteToAuditRequest>();
        autoMocker.CaptureRequest<WriteToAuditRequest>(capturedWriteToAudit.Add);

        var handler = autoMocker.CreateInstance<TokenIssuanceStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent);

        await handler.Run(fakeRequest);

        Assert.IsEmpty(capturedWriteToAudit);
    }
}
