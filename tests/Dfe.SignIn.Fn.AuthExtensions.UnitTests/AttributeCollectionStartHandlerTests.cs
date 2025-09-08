using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Fn.AuthExtensions.Constants;
using Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionStart;
using Microsoft.AspNetCore.Mvc;
using Moq.AutoMock;

namespace Dfe.SignIn.Fn.AuthExtensions.UnitTests;

[TestClass]
public class AttributeCollectionStartHandlerTests
{
    private static readonly AttributeCollectionStartEvent FakeEvent = new() {
        Type = "microsoft.graph.authenticationEvent.attributeCollectionStart",
        Data = new() {
            Type = "microsoft.graph.onAttributeCollectionStartCalloutData",
            AuthenticationContext = new() {
                CorrelationId = "36defbe5-3859-441e-9e60-ff0b1f77e9ab",
            },
            UserSignUpInfo = new() {
                Identities = [
                    new() {
                        SignInType = "emailAddress",
                        Issuer = "https://example.com",
                        IssuerAssignedId = "first.last@example.com",
                    },
                ],
            },
        },
    };

    [DataTestMethod]
    [DataRow("", "Invalid event type ''.")]
    [DataRow("microsoft.graph.invalidEvent", "Invalid event type 'microsoft.graph.invalidEvent'.")]
    public async Task Throws_WhenGivenUnexpectedEventType(
        string eventType, string expectedMessage)
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<AttributeCollectionStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Type = eventType,
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(()
            => handler.Run(fakeRequest));
        Assert.AreEqual(expectedMessage, exception.Message);
    }

    [DataTestMethod]
    [DataRow("", "Invalid callout data type ''.")]
    [DataRow("microsoft.graph.invalidCalloutDataType", "Invalid callout data type 'microsoft.graph.invalidCalloutDataType'.")]
    public async Task Throws_WhenGivenUnexpectedCalloutDataType(
        string calloutDataType, string expectedMessage)
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<AttributeCollectionStartHandler>();

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
    public async Task Throws_WhenUserHasNoIdentities()
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<AttributeCollectionStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                UserSignUpInfo = FakeEvent.Data.UserSignUpInfo with {
                    Identities = [],
                },
            },
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(()
            => handler.Run(fakeRequest));
        Assert.AreEqual("Has no identities.", exception.Message);
    }

    [TestMethod]
    public async Task Throws_WhenUserHasMultipleIdentities()
    {
        var autoMocker = new AutoMocker();
        var handler = autoMocker.CreateInstance<AttributeCollectionStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                UserSignUpInfo = FakeEvent.Data.UserSignUpInfo with {
                    Identities = [
                        new() {
                            SignInType = "first",
                            Issuer = "firstIssuer",
                            IssuerAssignedId = "firstId",
                        },
                        new() {
                            SignInType = "second",
                            Issuer = "secondIssuer",
                            IssuerAssignedId = "secondId",
                        },
                    ],
                },
            },
        });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(()
            => handler.Run(fakeRequest));
        Assert.AreEqual("Has multiple identities.", exception.Message);
    }

    [TestMethod]
    public async Task ReturnsBlockPageAction_WhenAttemptingToUseBlockedEmailAddress()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new CheckIsBlockedEmailAddressRequest {
                EmailAddress = "first.last@example.com",
            },
            new CheckIsBlockedEmailAddressResponse {
                IsBlocked = true,
            }
        );

        var handler = autoMocker.CreateInstance<AttributeCollectionStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent);

        var result = await handler.Run(fakeRequest);

        var okResult = TypeAssert.IsType<OkObjectResult>(result);
        var response = TypeAssert.IsType<ResponseObject>(okResult.Value);
        var data = TypeAssert.IsType<AttributeCollectionStartEventResponseData>(response.Data);
        Assert.HasCount(1, data.Actions);
        var blockPageAction = TypeAssert.IsType<ShowBlockPageAction>(data.Actions[0]);
        Assert.AreEqual(MessageConstants.BlockedEmailAddress, blockPageAction.Message);
    }

    [TestMethod]
    public async Task ReturnsContinueWithDefaultBehaviourAction_WhenSuccessful()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new CheckIsBlockedEmailAddressRequest {
                EmailAddress = "first.last@example.com",
            },
            new CheckIsBlockedEmailAddressResponse {
                IsBlocked = false,
            }
        );

        var handler = autoMocker.CreateInstance<AttributeCollectionStartHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent);

        var result = await handler.Run(fakeRequest);

        var okResult = TypeAssert.IsType<OkObjectResult>(result);
        var response = TypeAssert.IsType<ResponseObject>(okResult.Value);
        var data = TypeAssert.IsType<AttributeCollectionStartEventResponseData>(response.Data);
        Assert.HasCount(1, data.Actions);
        Assert.IsInstanceOfType<ContinueWithDefaultBehaviorAction>(data.Actions[0]);
    }
}
