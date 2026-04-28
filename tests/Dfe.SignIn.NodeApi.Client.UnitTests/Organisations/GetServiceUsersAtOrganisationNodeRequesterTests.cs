using System.Net;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Organisations;

[TestClass]
public sealed class GetServiceUsersAtOrganisationNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetServiceUsersAtOrganisationRequest,
            GetServiceUsersAtOrganisationNodeRequester
        >();
    }

    private static GetServiceUsersAtOrganisationNodeRequester CreateGetServiceUsersAtOrganisationNodeRequester(
        Dictionary<string, MappedResponse> responseMappings)
    {
        var organisationsHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var organisationsClient = new HttpClient(organisationsHandlerMock.Object) {
            BaseAddress = new Uri("http://organisations.localhost")
        };

        return new GetServiceUsersAtOrganisationNodeRequester(organisationsClient);
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Organisations API
            ["(GET) http://organisations.localhost/organisations/c173ec59-6670-4aca-b433-61c949a6f370/services/edd75704-0839-4f2a-be51-a6ecaf584019/users"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                [
                    {
                        "id": "51a50a75-e4fa-4b6e-9c72-581538ee5258",
                        "status": 1,
                        "organisation": {
                            "id": "c173ec59-6670-4aca-b433-61c949a6f370",
                            "name": "Test Organisation"
                        },
                        "role": {
                            "id": 1,
                            "name": "Admin"
                        }
                    },
                    {
                        "id": "a1b2c3d4-e5f6-47a8-9b0c-d1e2f3a4b5c6",
                        "status": 1,
                        "organisation": {
                            "id": "c173ec59-6670-4aca-b433-61c949a6f370",
                            "name": "Test Organisation"
                        },
                        "role": {
                            "id": 2,
                            "name": "User"
                        }
                    }
                ]
                """),
        };
    }

    [TestMethod]
    public async Task MakesExpectedRequestToGetServiceUsersAtOrganisation()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateGetServiceUsersAtOrganisationNodeRequester(responseMappings);

        var response = await interactor.InvokeAsync(new GetServiceUsersAtOrganisationRequest {
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.UserIds);

        var userIds = response.UserIds.ToList();
        Assert.AreEqual(2, userIds.Count);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), userIds[0]);
        Assert.AreEqual(new Guid("a1b2c3d4-e5f6-47a8-9b0c-d1e2f3a4b5c6"), userIds[1]);
    }

    [TestMethod]
    public async Task ReturnsEmptyUserIds_WhenNoUsersFound()
    {
        var interactor = CreateGetServiceUsersAtOrganisationNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/c173ec59-6670-4aca-b433-61c949a6f370/services/edd75704-0839-4f2a-be51-a6ecaf584019/users"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                []
                """),
        });

        var response = await interactor.InvokeAsync(new GetServiceUsersAtOrganisationRequest {
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.UserIds);
        Assert.AreEqual(0, response.UserIds.Count());
    }

    [TestMethod]
    public async Task ReturnsEmptyUserIds_WhenResponseIsNull()
    {
        var interactor = CreateGetServiceUsersAtOrganisationNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/c173ec59-6670-4aca-b433-61c949a6f370/services/edd75704-0839-4f2a-be51-a6ecaf584019/users"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                null
                """),
        });

        var response = await interactor.InvokeAsync(new GetServiceUsersAtOrganisationRequest {
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.UserIds);
        Assert.AreEqual(0, response.UserIds.Count());
    }

    [TestMethod]
    public async Task ReturnsSingleUser_WhenOnlyOneUserFound()
    {
        var interactor = CreateGetServiceUsersAtOrganisationNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/c173ec59-6670-4aca-b433-61c949a6f370/services/edd75704-0839-4f2a-be51-a6ecaf584019/users"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                [
                    {
                        "id": "51a50a75-e4fa-4b6e-9c72-581538ee5258",
                        "status": 1,
                        "organisation": {
                            "id": "c173ec59-6670-4aca-b433-61c949a6f370",
                            "name": "Test Organisation"
                        },
                        "role": {
                            "id": 1,
                            "name": "Admin"
                        }
                    }
                ]
                """),
        });

        var response = await interactor.InvokeAsync(new GetServiceUsersAtOrganisationRequest {
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.UserIds);
        var userIds = response.UserIds.ToList();
        Assert.AreEqual(1, userIds.Count);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), userIds[0]);
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateGetServiceUsersAtOrganisationNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/c173ec59-6670-4aca-b433-61c949a6f370/services/edd75704-0839-4f2a-be51-a6ecaf584019/users"] =
                new MappedResponse(HttpStatusCode.InternalServerError),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new GetServiceUsersAtOrganisationRequest {
                OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
                ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            }));
    }

    [TestMethod]
    public async Task Throws_WhenNotFound()
    {
        var interactor = CreateGetServiceUsersAtOrganisationNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/c173ec59-6670-4aca-b433-61c949a6f370/services/edd75704-0839-4f2a-be51-a6ecaf584019/users"] =
                new MappedResponse(HttpStatusCode.NotFound),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new GetServiceUsersAtOrganisationRequest {
                OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
                ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            }));
    }
}
