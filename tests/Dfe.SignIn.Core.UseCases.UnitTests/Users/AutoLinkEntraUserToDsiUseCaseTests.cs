using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.UseCases.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class AutoLinkEntraUserToDsiUseCaseTests
{
    private static readonly AutoLinkEntraUserToDsiRequest FakeRequest = new() {
        EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
        EmailAddress = "jo.bradford@example.com",
        GivenName = "Jo",
        Surname = "Bradford",
    };

    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            AutoLinkEntraUserToDsiRequest,
            AutoLinkEntraUserToDsiUseCase
        >();
    }

    #region User that is already linked

    [TestMethod]
    public async Task AlreadyLinked_Throws_WhenUserAccountIsInactive()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
            },
            new GetUserStatusResponse {
                UserExists = true,
                UserId = new Guid("cfc50de5-d4d5-42c4-b27c-ac2130f47ef2"),
                AccountStatus = AccountStatus.Inactive,
            }
        );

        var interactor = autoMocker.CreateInstance<AutoLinkEntraUserToDsiUseCase>();

        await Assert.ThrowsExactlyAsync<CannotLinkInactiveUserException>(()
            => interactor.InvokeAsync(FakeRequest));
    }

    [TestMethod]
    public async Task AlreadyLinked_ReturnsExistingUserId()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
            },
            new GetUserStatusResponse {
                UserExists = true,
                UserId = new Guid("cfc50de5-d4d5-42c4-b27c-ac2130f47ef2"),
                AccountStatus = AccountStatus.Active,
            }
        );

        var interactor = autoMocker.CreateInstance<AutoLinkEntraUserToDsiUseCase>();

        var response = await interactor.InvokeAsync(FakeRequest);

        var expectedDsiUserId = new Guid("cfc50de5-d4d5-42c4-b27c-ac2130f47ef2");
        Assert.AreEqual(expectedDsiUserId, response.UserId);
    }

    #endregion

    #region Linking to an existing user

    [TestMethod]
    public async Task LinkToExistingUser_Throws_WhenUserAccountIsInactive()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
            },
            new GetUserStatusResponse {
                UserExists = false,
            }
        );

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EmailAddress = "jo.bradford@example.com",
            },
            new GetUserStatusResponse {
                UserExists = true,
                UserId = new Guid("cfc50de5-d4d5-42c4-b27c-ac2130f47ef2"),
                AccountStatus = AccountStatus.Inactive,
            }
        );

        var interactor = autoMocker.CreateInstance<AutoLinkEntraUserToDsiUseCase>();

        await Assert.ThrowsExactlyAsync<CannotLinkInactiveUserException>(()
            => interactor.InvokeAsync(FakeRequest));
    }

    [TestMethod]
    public async Task LinkToExistingUser_LinksEntraUserToExistingDsiUser()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
            },
            new GetUserStatusResponse {
                UserExists = false,
            }
        );

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EmailAddress = "jo.bradford@example.com",
            },
            new GetUserStatusResponse {
                UserExists = true,
                UserId = new Guid("cfc50de5-d4d5-42c4-b27c-ac2130f47ef2"),
                AccountStatus = AccountStatus.Active,
            }
        );

        autoMocker.MockResponse(
            new LinkEntraUserToDsiRequest {
                DsiUserId = new Guid("cfc50de5-d4d5-42c4-b27c-ac2130f47ef2"),
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
                GivenName = "Jo",
                Surname = "Bradford",
            },
            new LinkEntraUserToDsiResponse()
        );

        var interactor = autoMocker.CreateInstance<AutoLinkEntraUserToDsiUseCase>();

        var response = await interactor.InvokeAsync(FakeRequest);

        var expectedDsiUserId = new Guid("cfc50de5-d4d5-42c4-b27c-ac2130f47ef2");
        Assert.AreEqual(expectedDsiUserId, response.UserId);
    }

    #endregion

    #region User does not exist yet

    [TestMethod]
    public async Task NewUser_CompletesPendingInvitation()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
            },
            new GetUserStatusResponse {
                UserExists = false,
            }
        );

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EmailAddress = "jo.bradford@example.com",
            },
            new GetUserStatusResponse {
                UserExists = false,
            }
        );

        autoMocker.MockResponse(
            new CompleteAnyPendingInvitationRequest {
                EmailAddress = "jo.bradford@example.com",
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
            },
            new CompleteAnyPendingInvitationResponse {
                WasCompleted = true,
                UserId = new Guid("96955687-6acb-4db4-9e59-eeca428e6938"),
            }
        );

        var interactor = autoMocker.CreateInstance<AutoLinkEntraUserToDsiUseCase>();

        var response = await interactor.InvokeAsync(FakeRequest);

        var expectedDsiUserId = new Guid("96955687-6acb-4db4-9e59-eeca428e6938");
        Assert.AreEqual(expectedDsiUserId, response.UserId);
    }

    [TestMethod]
    public async Task NewUser_CreatesNewUser()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
            },
            new GetUserStatusResponse {
                UserExists = false,
            }
        );

        autoMocker.MockResponse(
            new GetUserStatusRequest {
                EmailAddress = "jo.bradford@example.com",
            },
            new GetUserStatusResponse {
                UserExists = false,
            }
        );

        autoMocker.MockResponse(
            new CompleteAnyPendingInvitationRequest {
                EmailAddress = "jo.bradford@example.com",
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
            },
            new CompleteAnyPendingInvitationResponse {
                WasCompleted = false,
            }
        );

        autoMocker.MockResponse(
            new CreateUserRequest {
                EntraUserId = new Guid("c64bc171-ceef-4656-b22d-43918c14210f"),
                EmailAddress = "jo.bradford@example.com",
                GivenName = "Jo",
                Surname = "Bradford",
            },
            new CreateUserResponse {
                UserId = new Guid("2bc34d3e-8b9f-4c28-a400-84eb6e6b6bda"),
            }
        );

        var interactor = autoMocker.CreateInstance<AutoLinkEntraUserToDsiUseCase>();

        var response = await interactor.InvokeAsync(FakeRequest);

        var expectedDsiUserId = new Guid("2bc34d3e-8b9f-4c28-a400-84eb6e6b6bda");
        Assert.AreEqual(expectedDsiUserId, response.UserId);
    }

    #endregion
}
