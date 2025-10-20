using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Web.Profile.Services;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Profile.UnitTests.Services;

[TestClass]
public sealed class GraphApiChangeUserPasswordTests
{
    private static void SetupMockGraphServiceClientProvider(AutoMocker autoMocker)
    {
        var mockSerializationWriterFactory = autoMocker.GetMock<ISerializationWriterFactory>();
        mockSerializationWriterFactory
            .Setup(x => x.GetSerializationWriter(
                It.IsAny<string>()
            ))
            .Returns(new Mock<ISerializationWriter>().Object);

        var mockRequestAdapter = autoMocker.GetMock<IRequestAdapter>();
        mockRequestAdapter
            .Setup(x => x.SerializationWriterFactory)
            .Returns(mockSerializationWriterFactory.Object);

        autoMocker.GetMock<IPersonalGraphServiceFactory>()
            .Setup(x => x.GetClient(
                It.IsAny<AccessToken>()
            ))
            .Returns(new GraphServiceClient(mockRequestAdapter.Object, "https://test.localhost/"));
    }

    [TestMethod]
    public async Task Throws_WhenContextArgumentIsNull()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<GraphApiChangeUserPassword>();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => service.ChangePassword(context: null!));
    }

    [TestMethod]
    public async Task Throws_WhenGraphAccessTokenIsNull()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<GraphApiChangeUserPassword>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => service.ChangePassword(new SelfChangePasswordRequest {
                UserId = new Guid("6e2636e8-3937-491e-b4f9-3c65cce4a8e5"),
                CurrentPassword = "CurrentPass123",
                NewPassword = "NewPass123",
                ConfirmNewPassword = "NewPass123",
                GraphAccessToken = null,
            }));

        Assert.AreEqual("Missing user access token.", exception.Message);
    }

    [TestMethod]
    public async Task Throws_WhenConfirmedPasswordDoesNotMatch()
    {
        var autoMocker = new AutoMocker();
        var service = autoMocker.CreateInstance<GraphApiChangeUserPassword>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(()
            => service.ChangePassword(new SelfChangePasswordRequest {
                UserId = new Guid("6e2636e8-3937-491e-b4f9-3c65cce4a8e5"),
                CurrentPassword = "CurrentPass123",
                NewPassword = "NewPass123",
                ConfirmNewPassword = "Incorrect123",
                GraphAccessToken = new() {
                    Token = "FakeToken",
                    ExpiresOn = new DateTimeOffset(2025, 11, 10, 14, 52, 10, TimeSpan.Zero),
                },
            }));

        Assert.AreEqual("Confirmed password does not match new password.", exception.Message);
    }

    [TestMethod]
    public async Task SendsGraphApiRequest()
    {
        var autoMocker = new AutoMocker();
        SetupMockGraphServiceClientProvider(autoMocker);
        var service = autoMocker.CreateInstance<GraphApiChangeUserPassword>();

        await service.ChangePassword(new SelfChangePasswordRequest {
            UserId = new Guid("6e2636e8-3937-491e-b4f9-3c65cce4a8e5"),
            CurrentPassword = "CurrentPass123",
            NewPassword = "NewPass123",
            ConfirmNewPassword = "NewPass123",
            GraphAccessToken = new() {
                Token = "FakeToken",
                ExpiresOn = new DateTimeOffset(2025, 11, 10, 14, 52, 10, TimeSpan.Zero),
            },
        });
    }

    private static async Task<ODataError> CreateFakeErrorAsync(string message)
    {
        return (await KiotaJsonSerializer.DeserializeAsync<ODataError>(
            /*lang=json,strict*/ $$"""
            {
                "error": {
                    "message": "{{message}}"
                }
            }
            """
        ))!;
    }

    [TestMethod]
    public async Task Throws_InvalidRequestException_WhenCurrentPasswordWasIncorrect()
    {
        var autoMocker = new AutoMocker();
        SetupMockGraphServiceClientProvider(autoMocker);

        var mockRequestAdapter = autoMocker.GetMock<IRequestAdapter>();
        mockRequestAdapter
            .Setup(x => x.SendNoContentAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws(await CreateFakeErrorAsync("Incorrect current password. paramName: oldPassword"));

        var service = autoMocker.CreateInstance<GraphApiChangeUserPassword>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => service.ChangePassword(new SelfChangePasswordRequest {
                UserId = new Guid("6e2636e8-3937-491e-b4f9-3c65cce4a8e5"),
                CurrentPassword = "CurrentPass123",
                NewPassword = "NewPass123",
                ConfirmNewPassword = "NewPass123",
                GraphAccessToken = new() {
                    Token = "FakeToken",
                    ExpiresOn = new DateTimeOffset(2025, 11, 10, 14, 52, 10, TimeSpan.Zero),
                },
            }));

        InteractionAssert.HasValidationError(exception,
            "Please enter your current password",
            nameof(SelfChangePasswordRequest.CurrentPassword)
        );
    }

    [TestMethod]
    public async Task Throws_InvalidRequestException_WhenNewPasswordIsInvalid()
    {
        var autoMocker = new AutoMocker();
        SetupMockGraphServiceClientProvider(autoMocker);

        var mockRequestAdapter = autoMocker.GetMock<IRequestAdapter>();
        mockRequestAdapter
            .Setup(x => x.SendNoContentAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws(await CreateFakeErrorAsync("Password must be meet requirements. paramName: newPassword"));

        var service = autoMocker.CreateInstance<GraphApiChangeUserPassword>();

        var exception = await Assert.ThrowsExactlyAsync<InvalidRequestException>(()
            => service.ChangePassword(new SelfChangePasswordRequest {
                UserId = new Guid("6e2636e8-3937-491e-b4f9-3c65cce4a8e5"),
                CurrentPassword = "CurrentPass123",
                NewPassword = "NewPass123",
                ConfirmNewPassword = "NewPass123",
                GraphAccessToken = new() {
                    Token = "FakeToken",
                    ExpiresOn = new DateTimeOffset(2025, 11, 10, 14, 52, 10, TimeSpan.Zero),
                },
            }));

        InteractionAssert.HasValidationError(exception,
            "Password must be meet requirements.",
            nameof(SelfChangePasswordRequest.NewPassword)
        );
    }

    [TestMethod]
    public async Task Throws_WhenUnexpectedErrorOccurs()
    {
        var autoMocker = new AutoMocker();
        SetupMockGraphServiceClientProvider(autoMocker);

        var mockRequestAdapter = autoMocker.GetMock<IRequestAdapter>();
        mockRequestAdapter
            .Setup(x => x.SendNoContentAsync(
                It.IsAny<RequestInformation>(),
                It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                It.IsAny<CancellationToken>()
            ))
            .Throws(await CreateFakeErrorAsync("Something unexpected!"));

        var service = autoMocker.CreateInstance<GraphApiChangeUserPassword>();

        await Assert.ThrowsExactlyAsync<ODataError>(()
            => service.ChangePassword(new SelfChangePasswordRequest {
                UserId = new Guid("6e2636e8-3937-491e-b4f9-3c65cce4a8e5"),
                CurrentPassword = "CurrentPass123",
                NewPassword = "NewPass123",
                ConfirmNewPassword = "NewPass123",
                GraphAccessToken = new() {
                    Token = "FakeToken",
                    ExpiresOn = new DateTimeOffset(2025, 11, 10, 14, 52, 10, TimeSpan.Zero),
                },
            }));
    }
}
