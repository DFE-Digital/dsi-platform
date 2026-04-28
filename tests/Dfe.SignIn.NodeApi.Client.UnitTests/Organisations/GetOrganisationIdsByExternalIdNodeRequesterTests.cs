using System.Net;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Organisations;

[TestClass]
public sealed class GetOrganisationIdsByExternalIdNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetOrganisationIdsByExternalIdRequest,
            GetOrganisationIdsNodeRequester
        >();
    }

    private static GetOrganisationIdsNodeRequester CreateGetOrganisationIdsNodeRequester(
        Dictionary<string, MappedResponse> responseMappings)
    {
        var organisationsHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var organisationsClient = new HttpClient(organisationsHandlerMock.Object) {
            BaseAddress = new Uri("http://organisations.localhost")
        };

        return new GetOrganisationIdsNodeRequester(organisationsClient);
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Organisations API
            ["(GET) http://organisations.localhost/organisations/by-external-id/UKPRN/12345678"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                [
                    {
                        "id": "51a50a75-e4fa-4b6e-9c72-581538ee5258",
                        "name": "Test Organisation 1",
                        "legalName": "Test Organisation 1 Legal"
                    },
                    {
                        "id": "a1b2c3d4-e5f6-47a8-9b0c-d1e2f3a4b5c6",
                        "name": "Test Organisation 2",
                        "legalName": "Test Organisation 2 Legal"
                    }
                ]
                """),
        };
    }

    [TestMethod]
    public async Task MakesExpectedRequestToGetOrganisationIdsByExternalId()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateGetOrganisationIdsNodeRequester(responseMappings);

        var response = await interactor.InvokeAsync(new GetOrganisationIdsByExternalIdRequest {
            LookupKey = "UKPRN",
            LookupValue = "12345678",
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.OrganisationIds);

        var orgIds = response.OrganisationIds.ToList();
        Assert.AreEqual(2, orgIds.Count);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), orgIds[0]);
        Assert.AreEqual(new Guid("a1b2c3d4-e5f6-47a8-9b0c-d1e2f3a4b5c6"), orgIds[1]);
    }

    [TestMethod]
    public async Task ReturnsEmptyOrganisationIds_WhenNoOrganisationsFound()
    {
        var interactor = CreateGetOrganisationIdsNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/by-external-id/UKPRN/99999999"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                []
                """),
        });

        var response = await interactor.InvokeAsync(new GetOrganisationIdsByExternalIdRequest {
            LookupKey = "UKPRN",
            LookupValue = "99999999",
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.OrganisationIds);
        Assert.AreEqual(0, response.OrganisationIds.Count());
    }

    [TestMethod]
    public async Task ReturnsEmptyOrganisationIds_WhenResponseIsNull()
    {
        var interactor = CreateGetOrganisationIdsNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/by-external-id/UPIN/87654321"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                null
                """),
        });

        var response = await interactor.InvokeAsync(new GetOrganisationIdsByExternalIdRequest {
            LookupKey = "UPIN",
            LookupValue = "87654321",
        });

        Assert.IsNotNull(response);
        Assert.IsNull(response.OrganisationIds);
    }

    [TestMethod]
    public async Task ReturnsSingleOrganisationId_WhenOnlyOneOrganisationFound()
    {
        var interactor = CreateGetOrganisationIdsNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/by-external-id/UKPRN/11111111"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                [
                    {
                        "id": "51a50a75-e4fa-4b6e-9c72-581538ee5258",
                        "name": "Single Organisation",
                        "legalName": "Single Organisation Legal"
                    }
                ]
                """),
        });

        var response = await interactor.InvokeAsync(new GetOrganisationIdsByExternalIdRequest {
            LookupKey = "UKPRN",
            LookupValue = "11111111",
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.OrganisationIds);
        var orgIds = response.OrganisationIds.ToList();
        Assert.AreEqual(1, orgIds.Count);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), orgIds[0]);
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateGetOrganisationIdsNodeRequester(new() {
            ["(GET) http://organisations.localhost/organisations/by-external-id/UKPRN/99999999"] =
                new MappedResponse(HttpStatusCode.InternalServerError),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new GetOrganisationIdsByExternalIdRequest {
                LookupKey = "UKPRN",
                LookupValue = "99999999",
            }));
    }
}
