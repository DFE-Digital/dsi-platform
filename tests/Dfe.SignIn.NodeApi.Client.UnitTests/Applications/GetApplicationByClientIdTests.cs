using Dfe.SignIn.Core.InternalModels.Applications;
using Dfe.SignIn.Core.InternalModels.Applications.Interactions;
using Dfe.SignIn.NodeApi.Client.Applications;
using Dfe.SignIn.NodeApi.Client.Applications.Models;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Applications;

[TestClass]
public sealed class GetApplicationByClientId
{
    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetApplicationByClientIdRequest,
            GetApplicationByClientId_NodeApiRequester
        >();
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedOrganisation()
    {
        var mockDto = new ApplicationModelDto {
            Id = Guid.Parse("f3abb794-1399-4975-9c91-bf25d8ce9b4b"),
            Name = "mock-name",
            RelyingParty = new RelyingPartyModelDto {
                ClientId = "mock-client-id",
                ClientSecret = "mock-api-secret",
                GrantTypes = [],
                PostLogoutRedirectUris = [],
                RedirectUris = [],
                ResponseTypes = [],
                ServiceHome = "https://mock-home"
            }
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

        var controller = new GetApplicationByClientId_NodeApiRequester(client);

        var response = await controller.InvokeAsync(new GetApplicationByClientIdRequest {
            ClientId = "mock-client-id"
        });

        Assert.IsNotNull(response.Application);

        Assert.AreEqual(response.Application, new ApplicationModel {
            ApiSecret = mockDto.RelyingParty.ApiSecret,
            ClientId = mockDto.RelyingParty.ClientId,
            Description = mockDto.Description,
            Id = mockDto.Id,
            Name = mockDto.Name,
            ServiceHomeUrl = new Uri(mockDto.RelyingParty.ServiceHome),
            IsExternalService = mockDto.IsExternalService,
            IsHiddenService = mockDto.IsHiddenService,
            IsIdOnlyService = mockDto.IsIdOnlyService
        });
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsEmptyCollectionOfApplicationsWhenNotFound()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var client = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://mock.localhost")
        };

        var controller = new GetApplicationByClientId_NodeApiRequester(client);

        var response = await controller.InvokeAsync(new GetApplicationByClientIdRequest {
            ClientId = "mock-client-id"
        });

        Assert.IsNull(response.Application);
    }
}
