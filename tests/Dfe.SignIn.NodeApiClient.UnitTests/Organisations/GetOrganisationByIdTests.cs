using AutoMapper;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.PublicModels.Organisations;
using Dfe.SignIn.NodeApiClient.MappingProfiles;
using Dfe.SignIn.NodeApiClient.Organisations;
using Dfe.SignIn.NodeApiClient.Organisations.Models;
using Dfe.SignIn.NodeApiClient.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApiClient.UnitTests.Organisations;

[TestClass]
public class GetOrganisationByIdTests
{
    private IMapper? mapper;

    [TestInitialize]
    public void Setup()
    {
        var mapperConfiguration = new MapperConfiguration(options => options.AddProfile<OrganisationProfile>());
        this.mapper = mapperConfiguration.CreateMapper();
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedOrganisation()
    {
        var mockDto = new OrganisationByIdDto() {
            Id = Guid.Parse("3a939152-d229-4ac2-9ffa-61cd85576f0e"),
            Name = "mock-name",
            Status = 1,
            Category = "001",
        };

        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(mockDto), System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://mock.localhost")
        };

        var controller = new GetOrganisationById_NodeApiRequester(client, this.mapper!);

        var response = await controller.InvokeAsync(new Core.Models.Organisations.Interactions.GetOrganisationByIdRequest {
            OrganisationId = Guid.Empty
        });

        Assert.AreEqual(response.Organisation, new OrganisationModel {
            Id = mockDto.Id,
            Name = mockDto.Name,
            Status = OrganisationStatus.Open,
            Category = OrganisationCategory.Establishment,
        });
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsNullOrganisationWhenNotFound()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var client = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://mock.localhost")
        };

        var controller = new GetOrganisationById_NodeApiRequester(client, this.mapper!);

        var response = await controller.InvokeAsync(new Core.Models.Organisations.Interactions.GetOrganisationByIdRequest {
            OrganisationId = Guid.Empty
        });

        Assert.IsNull(response.Organisation);
    }
}
