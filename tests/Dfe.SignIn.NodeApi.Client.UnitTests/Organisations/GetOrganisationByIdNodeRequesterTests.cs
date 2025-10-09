using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.NodeApi.Client.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations.Models;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Organisations;

[TestClass]
public sealed class GetOrganisationByIdNodeRequesterTests
{
    [TestMethod]
    public async Task ReturnsExpectedOrganisation()
    {
        var mockDto = new OrganisationByIdDto() {
            Id = Guid.Parse("3a939152-d229-4ac2-9ffa-61cd85576f0e"),
            Name = "mock-name",
            Status = 1,
            Category = OrganisationConstants.CategoryId_Establishment,
        };

        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(mockDto), System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var organisationsClient = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://organisations.localhost")
        };

        var controller = new GetOrganisationByIdNodeRequester(organisationsClient);

        var response = await controller.InvokeAsync(new GetOrganisationByIdRequest {
            OrganisationId = Guid.Empty
        });

        Assert.AreEqual(response.Organisation, new Organisation {
            Id = mockDto.Id,
            Name = mockDto.Name,
            Status = OrganisationStatus.Open,
            Category = OrganisationCategory.Establishment,
        });
    }

    [TestMethod]
    public async Task ReturnsNullOrganisation_WhenNotFound()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var organisationsClient = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://organisations.localhost")
        };

        var controller = new GetOrganisationByIdNodeRequester(organisationsClient);

        var response = await controller.InvokeAsync(new GetOrganisationByIdRequest {
            OrganisationId = Guid.Empty
        });

        Assert.IsNull(response.Organisation);
    }
}
