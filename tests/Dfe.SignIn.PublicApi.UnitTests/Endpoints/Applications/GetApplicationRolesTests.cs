using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.PublicApi.Endpoints.Applications;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Applications;

[TestClass]
public class GetApplicationRolesTests
{
    private const string FakeClientId = "test-client-id";
    private static readonly Guid FakeApplicationId = new("550e8400-e29b-41d4-a716-446655440000");

    private static readonly Application FakeApplication = new() {
        Id = FakeApplicationId,
        ClientId = FakeClientId,
        Name = "Test Application",
        Description = "A test application",
        IsExternalService = false,
        IsHiddenService = false,
        IsIdOnlyService = false,
    };

    private static readonly ApplicationRole FakeCoreRole1 = new() {
        Id = new Guid("a5a8e401-e29b-41d4-a716-446655440001"),
        Code = "DSI_Child_One",
        Name = "DSI Child One",
        NumericId = 1,
        Status = ApplicationRoleStatus.Active,
        Parent = null
    };

    private static readonly ApplicationRole FakeCoreRole2 = new() {
        Id = new Guid("b6b9e402-e29b-41d4-a716-446655440002"),
        Code = "gias_establishment",
        Name = "GIAS - establishment",
        NumericId = 2,
        Status = ApplicationRoleStatus.Active,
        Parent = null
    };

    private static readonly GetRolesOfApplicationResponse FakeRolesResponse = new() {
        Roles = [FakeCoreRole1, FakeCoreRole2]
    };

    private static AutoMocker CreateAutoMocker() => new();

    [TestMethod]
    public async Task ResolvesClientIdToApplicationId()
    {
        var autoMocker = CreateAutoMocker();

        GetApplicationByClientIdRequest? capturedRequest = null;
        autoMocker.CaptureRequest<GetApplicationByClientIdRequest>(
            req => capturedRequest = req,
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetRolesOfApplicationRequest>(FakeRolesResponse);

        await ApplicationEndpoints.GetApplicationRoles(FakeClientId, autoMocker.Get<IInteractionDispatcher>());

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(FakeClientId, capturedRequest.ClientId);
    }

    [TestMethod]
    public async Task DispatchesGetRolesOfApplicationRequest()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );

        GetRolesOfApplicationRequest? capturedRequest = null;
        autoMocker.CaptureRequest<GetRolesOfApplicationRequest>(
            req => capturedRequest = req,
            FakeRolesResponse
        );

        await ApplicationEndpoints.GetApplicationRoles(FakeClientId, autoMocker.Get<IInteractionDispatcher>());

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(FakeApplicationId, capturedRequest.ApplicationId);
    }

    [TestMethod]
    public async Task ReturnsMappedRoles()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetRolesOfApplicationRequest>(FakeRolesResponse);

        var result = (await ApplicationEndpoints.GetApplicationRoles(FakeClientId, autoMocker.Get<IInteractionDispatcher>())).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(r => r.Code == "DSI_Child_One" && r.Name == "DSI Child One" && r.Status == "Active"));
        Assert.IsTrue(result.Any(r => r.Code == "gias_establishment" && r.Name == "GIAS - establishment" && r.Status == "Active"));
    }

    [TestMethod]
    public async Task ReturnsEmptyListWhenNoRoles()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetRolesOfApplicationRequest>(
            new GetRolesOfApplicationResponse { Roles = [] }
        );

        var result = (await ApplicationEndpoints.GetApplicationRoles(FakeClientId, autoMocker.Get<IInteractionDispatcher>())).ToList();

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task StatusIsMappedAsString()
    {
        var autoMocker = CreateAutoMocker();

        var inactiveRole = new ApplicationRole {
            Id = Guid.NewGuid(),
            Code = "inactive_role",
            Name = "Inactive Role",
            NumericId = 3,
            Status = ApplicationRoleStatus.Inactive,
            Parent = null
        };

        autoMocker.MockResponse<GetApplicationByClientIdRequest>(
            new GetApplicationByClientIdResponse { Application = FakeApplication }
        );
        autoMocker.MockResponse<GetRolesOfApplicationRequest>(
            new GetRolesOfApplicationResponse { Roles = [inactiveRole] }
        );

        var result = (await ApplicationEndpoints.GetApplicationRoles(FakeClientId, autoMocker.Get<IInteractionDispatcher>())).ToList();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Inactive", result[0].Status);
    }
}
