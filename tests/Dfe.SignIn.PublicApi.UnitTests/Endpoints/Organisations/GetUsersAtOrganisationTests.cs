using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Endpoints.Organisations;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Organisations;

[TestClass]
public sealed class GetUsersAtOrganisationTests
{
    private static readonly string FakeUkprn = "10000001";
    private static readonly Guid FakeApplicationId = new("a1b2c3d4-e5f6-4789-a1b2-c3d4e5f6a1b2");
    private static readonly Guid FakeOrganisationId1 = new("b2c3d4e5-f6a1-47b2-c3d4-e5f6a1b2c3d4");
    private static readonly Guid FakeOrganisationId2 = new("c3d4e5f6-a1b2-47c3-d4e5-f6a1b2c3d4e5");
    private static readonly Guid FakeUserId1 = new("d4e5f6a1-b2c3-47d4-e5f6-a1b2c3d4e5f6");
    private static readonly Guid FakeUserId2 = new("e5f6a1b2-c3d4-47e5-f6a1-b2c3d4e5f6a1");

    private const string FakeApplicationClientId = "test-service-client";

    private static readonly GetUserProfileResponse FakeUserProfile1 = new() {
        IsEntra = false,
        IsInternalUser = false,
        EmailAddress = "user1@example.com",
        FirstName = "John",
        LastName = "Doe",
    };

    private static AutoMocker CreateAutoMocker()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IClientSession>()
            .Setup(x => x.ClientId)
            .Returns(FakeApplicationClientId);

        return autoMocker;
    }

    private static Application CreateFakeApplication()
    {
        return new() {
            Id = FakeApplicationId,
            ClientId = FakeApplicationClientId,
            Name = "Test App",
            IsExternalService = true,
            IsIdOnlyService = false,
            IsHiddenService = false,
        };
    }

    [TestMethod]
    public async Task GetUsersAtOrganisation_ReturnsNotFound_WhenApplicationIdNotFound()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse {
                Application = null,
            }
        );

        var result = await OrganisationEndpoints.GetUsersAtOrganisation(
            FakeUkprn,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsInstanceOfType(result.Result, typeof(NotFound));
    }

    [TestMethod]
    public async Task GetUsersAtOrganisation_ReturnsNotFound_WhenNoOrganisationIdsFound()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse {
                Application = CreateFakeApplication(),
            }
        );

        autoMocker.MockResponse<GetOrganisationIdsByExternalIdRequest>(
            new GetOrganisationIdsByExternalIdResponse {
                OrganisationIds = [],
            }
        );

        var result = await OrganisationEndpoints.GetUsersAtOrganisation(
            FakeUkprn,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsInstanceOfType(result.Result, typeof(NotFound));
    }

    [TestMethod]
    public async Task GetUsersAtOrganisation_ReturnsNotFound_WhenOrganisationIdsResponseIsNull()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse {
                Application = CreateFakeApplication(),
            }
        );

        autoMocker.MockResponse<GetOrganisationIdsByExternalIdRequest>(
            new GetOrganisationIdsByExternalIdResponse {
                OrganisationIds = null,
            }
        );

        var result = await OrganisationEndpoints.GetUsersAtOrganisation(
            FakeUkprn,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsInstanceOfType(result.Result, typeof(NotFound));
    }

    [TestMethod]
    public async Task GetUsersAtOrganisation_ReturnsSuccessfulResponse_WithUsers()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse {
                Application = CreateFakeApplication(),
            }
        );

        autoMocker.MockResponse<GetOrganisationIdsByExternalIdRequest>(
            new GetOrganisationIdsByExternalIdResponse {
                OrganisationIds = [FakeOrganisationId1],
            }
        );

        autoMocker.MockResponse<GetServiceUsersAtOrganisationRequest>(
            new GetServiceUsersAtOrganisationResponse([FakeUserId1, FakeUserId2])
        );

        autoMocker.MockResponse<GetRolesOfUserRequest>(
            new GetRolesOfUserResponse {
                Roles = ["Admin", "Viewer"],
            }
        );

        autoMocker.MockResponse<GetUserProfileRequest>(
            FakeUserProfile1
        );

        var result = await OrganisationEndpoints.GetUsersAtOrganisation(
            FakeUkprn,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsInstanceOfType(result.Result, typeof(Ok<GetUsersAtOrganisationResponse>));

        var okResult = (Ok<GetUsersAtOrganisationResponse>)result.Result;
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(FakeUkprn.ToString(), okResult.Value.Ukprn);
        Assert.IsNotNull(okResult.Value.Users);
    }

    [TestMethod]
    public async Task GetUsersAtOrganisation_ReturnsEmptyUserList_WhenNoUsersAtOrganisation()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse {
                Application = CreateFakeApplication(),
            }
        );

        autoMocker.MockResponse<GetOrganisationIdsByExternalIdRequest>(
            new GetOrganisationIdsByExternalIdResponse {
                OrganisationIds = [FakeOrganisationId1],
            }
        );

        autoMocker.MockResponse<GetServiceUsersAtOrganisationRequest>(
            new GetServiceUsersAtOrganisationResponse([])
        );

        var result = await OrganisationEndpoints.GetUsersAtOrganisation(
            FakeUkprn,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsInstanceOfType(result.Result, typeof(Ok<GetUsersAtOrganisationResponse>));

        var okResult = (Ok<GetUsersAtOrganisationResponse>)result.Result;
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(FakeUkprn.ToString(), okResult.Value.Ukprn);
        Assert.AreEqual(0, okResult.Value.Users.Count);
    }

    [TestMethod]
    public async Task GetUsersAtOrganisation_HandlesMultipleOrganisations()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse {
                Application = CreateFakeApplication(),
            }
        );

        autoMocker.MockResponse<GetOrganisationIdsByExternalIdRequest>(
            new GetOrganisationIdsByExternalIdResponse {
                OrganisationIds = [FakeOrganisationId1, FakeOrganisationId2],
            }
        );

        autoMocker.MockResponse<GetServiceUsersAtOrganisationRequest>(
            new GetServiceUsersAtOrganisationResponse([FakeUserId1])
        );

        autoMocker.MockResponse<GetRolesOfUserRequest>(
            new GetRolesOfUserResponse {
                Roles = ["Admin"],
            }
        );

        autoMocker.MockResponse<GetUserProfileRequest>(
            FakeUserProfile1
        );

        var result = await OrganisationEndpoints.GetUsersAtOrganisation(
            FakeUkprn,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsInstanceOfType(result.Result, typeof(Ok<GetUsersAtOrganisationResponse>));

        var okResult = (Ok<GetUsersAtOrganisationResponse>)result.Result;
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(FakeUkprn.ToString(), okResult.Value.Ukprn);
    }

    [TestMethod]
    public async Task GetUsersAtOrganisation_IncludesAllUserRoles()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse {
                Application = CreateFakeApplication(),
            }
        );

        autoMocker.MockResponse<GetOrganisationIdsByExternalIdRequest>(
            new GetOrganisationIdsByExternalIdResponse {
                OrganisationIds = [FakeOrganisationId1],
            }
        );

        autoMocker.MockResponse<GetServiceUsersAtOrganisationRequest>(
            new GetServiceUsersAtOrganisationResponse([FakeUserId1])
        );

        var rolesList = new[] { "Admin", "Editor", "Viewer" };
        autoMocker.MockResponse<GetRolesOfUserRequest>(
            new GetRolesOfUserResponse {
                Roles = rolesList,
            }
        );

        autoMocker.MockResponse<GetUserProfileRequest>(
            FakeUserProfile1
        );

        var result = await OrganisationEndpoints.GetUsersAtOrganisation(
            FakeUkprn,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsInstanceOfType(result.Result, typeof(Ok<GetUsersAtOrganisationResponse>));

        var okResult = (Ok<GetUsersAtOrganisationResponse>)result.Result;
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(1, okResult.Value.Users.Count);

        var user = okResult.Value.Users.First();
        Assert.AreEqual(FakeUserProfile1.FirstName, user.FirstName);
        Assert.AreEqual(FakeUserProfile1.LastName, user.LastName);
        Assert.AreEqual(FakeUserProfile1.EmailAddress, user.Email);
        Assert.AreEqual(3, user.Roles.Count);
    }

    [TestMethod]
    public async Task GetUsersAtOrganisation_ReturnsExpectedUserDetails()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse {
                Application = CreateFakeApplication(),
            }
        );

        autoMocker.MockResponse<GetOrganisationIdsByExternalIdRequest>(
            new GetOrganisationIdsByExternalIdResponse {
                OrganisationIds = [FakeOrganisationId1],
            }
        );

        autoMocker.MockResponse<GetServiceUsersAtOrganisationRequest>(
            new GetServiceUsersAtOrganisationResponse([FakeUserId1])
        );

        autoMocker.MockResponse<GetRolesOfUserRequest>(
            new GetRolesOfUserResponse {
                Roles = ["Admin"],
            }
        );

        autoMocker.MockResponse<GetUserProfileRequest>(
            FakeUserProfile1
        );

        var result = await OrganisationEndpoints.GetUsersAtOrganisation(
            FakeUkprn,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        var okResult = (Ok<GetUsersAtOrganisationResponse>)result.Result;
        var user = okResult.Value!.Users[0];

        Assert.AreEqual(FakeUserProfile1.EmailAddress, user.Email);
        Assert.AreEqual(FakeUserProfile1.FirstName, user.FirstName);
        Assert.AreEqual(FakeUserProfile1.LastName, user.LastName);
    }
}
