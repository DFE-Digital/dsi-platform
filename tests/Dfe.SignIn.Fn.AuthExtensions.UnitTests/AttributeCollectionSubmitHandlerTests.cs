using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Fn.AuthExtensions.Constants;
using Dfe.SignIn.Fn.AuthExtensions.OnAttributeCollectionSubmit;
using Microsoft.AspNetCore.Mvc;
using Moq.AutoMock;

namespace Dfe.SignIn.Fn.AuthExtensions.UnitTests;

[TestClass]
public class AttributeCollectionSubmitHandlerTests
{
    private static readonly AttributeCollectionSubmitEvent FakeEvent = new() {
        Type = "microsoft.graph.authenticationEvent.attributeCollectionSubmit",
        Data = new() {
            Type = "microsoft.graph.onAttributeCollectionSubmitCalloutData",
            AuthenticationContext = new() {
                CorrelationId = "9c146a5c-6e03-4ee9-b25b-96c4a2309213",
            },
            UserSignUpInfo = new() {
                Attributes = new() {
                    GivenName = new() {
                        Type = "microsoft.graph.stringDirectoryAttributeValue",
                        Value = "Jo",
                        AttributeType = "builtIn",
                    },
                    Surname = new() {
                        Type = "microsoft.graph.stringDirectoryAttributeValue",
                        Value = "Bradford",
                        AttributeType = "builtIn",
                    }
                },
            },
        },
    };

    private static void SetupMockInteractionValidationResults(AutoMocker autoMocker)
    {
        autoMocker.Use<IInteractionValidator>(new InteractionValidator(autoMocker));
    }

    [TestMethod]
    [DataRow("", "Invalid event type ''.")]
    [DataRow("microsoft.graph.invalidEvent", "Invalid event type 'microsoft.graph.invalidEvent'.")]
    public async Task Throws_WhenGivenUnexpectedEventType(
        string eventType, string expectedMessage)
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidationResults(autoMocker);
        var handler = autoMocker.CreateInstance<AttributeCollectionSubmitHandler>();

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
        SetupMockInteractionValidationResults(autoMocker);
        var handler = autoMocker.CreateInstance<AttributeCollectionSubmitHandler>();

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
    public async Task Throws_WhenGivenNameAttributeHasWrongType()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidationResults(autoMocker);
        var handler = autoMocker.CreateInstance<AttributeCollectionSubmitHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                UserSignUpInfo = FakeEvent.Data.UserSignUpInfo with {
                    Attributes = FakeEvent.Data.UserSignUpInfo.Attributes with {
                        GivenName = new() {
                            Type = "microsoft.graph.unknownType",
                            Value = "Jo",
                            AttributeType = "custom",
                        },
                    },
                },
            },
        });

        var exception = await Assert.ThrowsAsync<ValidationException>(()
            => handler.Run(fakeRequest));
        Assert.AreEqual("Invalid value for attribute 'givenName'.", exception.Message);
    }

    [TestMethod]
    public async Task ReturnsValidationError_WhenGivenNameIsInvalid()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidationResults(autoMocker);
        var handler = autoMocker.CreateInstance<AttributeCollectionSubmitHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                UserSignUpInfo = FakeEvent.Data.UserSignUpInfo with {
                    Attributes = FakeEvent.Data.UserSignUpInfo.Attributes with {
                        GivenName = FakeEvent.Data.UserSignUpInfo.Attributes.GivenName with {
                            Value = "[Jo]",
                        },
                    },
                },
            },
        });

        var result = await handler.Run(fakeRequest);

        var okResult = TypeAssert.IsType<OkObjectResult>(result);
        var response = TypeAssert.IsType<ResponseObject>(okResult.Value);
        var data = TypeAssert.IsType<AttributeCollectionSubmitEventResponseData>(response.Data);
        Assert.HasCount(1, data.Actions);
        var showValidationErrorAction = TypeAssert.IsType<ShowValidationErrorAction>(data.Actions[0]);
        Assert.AreEqual("Please fix the below errors to proceed.", showValidationErrorAction.Message);
        Assert.AreEqual("Enter a valid first name", showValidationErrorAction.AttributeErrors["givenName"]);
    }

    [TestMethod]
    public async Task Throws_WhenSurnameAttributeHasWrongType()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidationResults(autoMocker);
        var handler = autoMocker.CreateInstance<AttributeCollectionSubmitHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                UserSignUpInfo = FakeEvent.Data.UserSignUpInfo with {
                    Attributes = FakeEvent.Data.UserSignUpInfo.Attributes with {
                        Surname = new() {
                            Type = "microsoft.graph.unknownType",
                            Value = "Bradford",
                            AttributeType = "custom",
                        },
                    },
                },
            },
        });

        var exception = await Assert.ThrowsAsync<ValidationException>(()
            => handler.Run(fakeRequest));
        Assert.AreEqual("Invalid value for attribute 'surname'.", exception.Message);
    }

    [TestMethod]
    public async Task ReturnsValidationError_WhenSurnameIsInvalid()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidationResults(autoMocker);
        var handler = autoMocker.CreateInstance<AttributeCollectionSubmitHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent with {
            Data = FakeEvent.Data with {
                UserSignUpInfo = FakeEvent.Data.UserSignUpInfo with {
                    Attributes = FakeEvent.Data.UserSignUpInfo.Attributes with {
                        Surname = FakeEvent.Data.UserSignUpInfo.Attributes.Surname with {
                            Value = "[Bradford]",
                        },
                    },
                },
            },
        });

        var result = await handler.Run(fakeRequest);

        var okResult = TypeAssert.IsType<OkObjectResult>(result);
        var response = TypeAssert.IsType<ResponseObject>(okResult.Value);
        var data = TypeAssert.IsType<AttributeCollectionSubmitEventResponseData>(response.Data);
        Assert.HasCount(1, data.Actions);
        var showValidationErrorAction = TypeAssert.IsType<ShowValidationErrorAction>(data.Actions[0]);
        Assert.AreEqual("Please fix the below errors to proceed.", showValidationErrorAction.Message);
        Assert.AreEqual("Enter a valid last name", showValidationErrorAction.AttributeErrors["surname"]);
    }

    [TestMethod]
    public async Task ReturnsActionToModifyDisplayName()
    {
        var autoMocker = new AutoMocker();
        SetupMockInteractionValidationResults(autoMocker);
        var handler = autoMocker.CreateInstance<AttributeCollectionSubmitHandler>();

        var fakeRequest = HttpServerMocking.CreateJsonRequest(FakeEvent);

        var result = await handler.Run(fakeRequest);

        var okResult = TypeAssert.IsType<OkObjectResult>(result);
        var response = TypeAssert.IsType<ResponseObject>(okResult.Value);
        var data = TypeAssert.IsType<AttributeCollectionSubmitEventResponseData>(response.Data);
        Assert.HasCount(1, data.Actions);
        var modifyAction = TypeAssert.IsType<ModifyAttributeValuesAction>(data.Actions[0]);
        Assert.AreEqual("BRADFORD, Jo", modifyAction.Attributes[UserAttributeConstants.DisplayName]);
    }
}
