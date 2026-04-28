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
        var accessClient = new HttpClient(accessHandlerMock.Object)
        {
            BaseAddress = new Uri("http://access.localhost")
        };

        return new GetRolesOfUserNodeRequester(accessClient);
    }

    [TestMethod]
    public async Task ReturnsEmptyRoles_WhenRolesPropertyIsNull()
    {
        var interactor = CreateGetRolesOfUserNodeRequester(new()
        {
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

        var response = await interactor.InvokeAsync(new GetRolesOfUserRequest
        {
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
        var interactor = CreateGetRolesOfUserNodeRequester(new()
        {
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                null
                """),
        });

        var response = await interactor.InvokeAsync(new GetRolesOfUserRequest
        {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Roles);
        Assert.AreEqual(0, response.Roles.Count());
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateGetRolesOfUserNodeRequester(new()
        {
            ["(GET) http://access.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258/services/edd75704-0839-4f2a-be51-a6ecaf584019/organisations/c173ec59-6670-4aca-b433-61c949a6f370"] =
                new MappedResponse(HttpStatusCode.InternalServerError),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => interactor.InvokeAsync(new GetRolesOfUserRequest
        {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            ApplicationId = new Guid("edd75704-0839-4f2a-be51-a6ecaf584019"),
            OrganisationId = new Guid("c173ec59-6670-4aca-b433-61c949a6f370"),
        }));
    }
}
