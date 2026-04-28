using System.Net;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.NodeApi.Client.Access;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Access;

[TestClass]
public sealed class GetRolesOfUserNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetRolesOfUserRequest,
            GetRolesOfUserNodeRequester
        >();
    }

    private static GetRolesOfUserNodeRequester CreateGetRolesOfUserNodeRequester(
        Dictionary<string, MappedResponse> responseMappings)
    {
        var accessHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var accessClient = new HttpClient(accessHandlerMock.Object) {
            BaseAddress = new Uri("http://access.localhost")
        };

        return new GetRolesOfUserNodeRequester(accessClient);
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Access API
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "userId": "51a50a75-e4fa-4b6e-9c72-581538ee5258",
                    "serviceId": "edd75704-0839-4f2a-be51-a6ecaf584019",
                    "organisationId": "c173ec59-6670-4aca-b433-61c949a6f370",
                    "roles": [
                        {
                            "id": "a1b2c3d4-e5f6-47a8-9b0c-d1e2f3a4b5c6",
                            "name": "Administrator",
                            "code": "ADMIN",
                            "numericId": 1
                        },
                        {
                            "id": "b2c3d4e5-f6a7-48b9-ac1d-e2f3a4b5c6d7",
                            "name": "Editor",
                            "code": "EDITOR",
                            "numericId": 2
                        }
                    ],
                    "identifiers": []
                }
                """),
        };
    }

    [TestMethod]
    public async Task MakesExpectedRequestToGetRolesOfUser()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateGetRolesOfUserNodeRequester(responseMappings);

        var response = await interactor.InvokeAsync(new GetRolesOfUserRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Roles);

        var roles = response.Roles.ToList();
        Assert.AreEqual(2, roles.Count);
        Assert.AreEqual("ADMIN", roles[0]);
        Assert.AreEqual("EDITOR", roles[1]);
    }

    [TestMethod]
    public async Task ReturnsEmptyRoles_WhenNoRolesFound()
    {
        var interactor = CreateGetRolesOfUserNodeRequester(new() {
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "userId": "51a50a75-e4fa-4b6e-9c72-581538ee5258",
                    "serviceId": "edd75704-0839-4f2a-be51-a6ecaf584019",
                    "organisationId": "c173ec59-6670-4aca-b433-61c949a6f370",
                    "roles": [],
                    "identifiers": []
                }
                """),
        });

        var response = await interactor.InvokeAsync(new GetRolesOfUserRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Roles);
        Assert.AreEqual(0, response.Roles.Count());
    }

    [TestMethod]
    public async Task ReturnsEmptyRoles_WhenRolesPropertyIsNull()
    {
        var interactor = CreateGetRolesOfUserNodeRequester(new() {
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "userId": "51a50a75-e4fa-4b6e-9c72-581538ee5258",
                    "serviceId": "edd75704-0839-4f2a-be51-a6ecaf584019",
                    "organisationId": "c173ec59-6670-4aca-b433-61c949a6f370",
                    "roles": null,
                    "identifiers": []
                }
                """),
        });

        var response = await interactor.InvokeAsync(new GetRolesOfUserRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Roles);
        Assert.AreEqual(0, response.Roles.Count());
    }

    [TestMethod]
    public async Task ReturnsEmptyRoles_WhenResponseIsNull()
    {
        var interactor = CreateGetRolesOfUserNodeRequester(new() {
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                null
                """),
        });

        var response = await interactor.InvokeAsync(new GetRolesOfUserRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Roles);
        Assert.AreEqual(0, response.Roles.Count());
    }

    [TestMethod]
    public async Task ReturnsSingleRole_WhenOnlyOneRoleFound()
    {
        var interactor = CreateGetRolesOfUserNodeRequester(new() {
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "userId": "51a50a75-e4fa-4b6e-9c72-581538ee5258",
                    "serviceId": "edd75704-0839-4f2a-be51-a6ecaf584019",
                    "organisationId": "c173ec59-6670-4aca-b433-61c949a6f370",
                    "roles": [
                        {
                            "id": "a1b2c3d4-e5f6-47a8-9b0c-d1e2f3a4b5c6",
                            "name": "Viewer",
                            "code": "VIEWER",
                            "numericId": 3
                        }
                    ],
                    "identifiers": []
                }
                """),
        });

        var response = await interactor.InvokeAsync(new GetRolesOfUserRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Roles);
        var roles = response.Roles.ToList();
        Assert.AreEqual(1, roles.Count);
        Assert.AreEqual("VIEWER", roles[0]);
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateGetRolesOfUserNodeRequester(new() {
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.InternalServerError),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new GetRolesOfUserRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
                OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
            }));
    }

    [TestMethod]
    public async Task Throws_WhenNotFound()
    {
        var interactor = CreateGetRolesOfUserNodeRequester(new() {
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.NotFound),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new GetRolesOfUserRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
                OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
            }));
    }
}
