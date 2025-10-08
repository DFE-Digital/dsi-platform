using System.Net;
using System.Text;
using System.Text.Json;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.NodeApi.Client.Applications;
using Dfe.SignIn.NodeApi.Client.Applications.Models;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Applications;

[TestClass]
public sealed class GetApplicationApiConfigurationNodeRequesterTests
{
    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetApplicationApiConfigurationRequest,
            GetApplicationApiConfigurationNodeRequester
        >();
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedOrganisation()
    {
        var mockDto = new ApplicationDto {
            Id = Guid.Parse("f3abb794-1399-4975-9c91-bf25d8ce9b4b"),
            Name = "mock-name",
            RelyingParty = new RelyingPartyDto {
                ClientId = "mock-client-id",
                ApiSecret = "mock-api-secret",
                ClientSecret = "mock-client-secret",
                GrantTypes = [],
                PostLogoutRedirectUris = [],
                RedirectUris = [],
                ResponseTypes = [],
                ServiceHome = "https://mock-home"
            }
        };

        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(JsonSerializer.Serialize(mockDto), Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var applicationsClient = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://applications.localhost")
        };

        var controller = new GetApplicationApiConfigurationNodeRequester(applicationsClient);

        var response = await controller.InvokeAsync(new GetApplicationApiConfigurationRequest {
            ClientId = "mock-client-id"
        });

        Assert.IsNotNull(response.Configuration);

        Assert.AreEqual(response.Configuration, new ApplicationApiConfiguration {
            ClientId = mockDto.RelyingParty.ClientId,
            ApiSecret = mockDto.RelyingParty.ApiSecret!,
        });
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenApplicationNotFound()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var applicationsClient = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://applications.localhost")
        };

        var controller = new GetApplicationApiConfigurationNodeRequester(applicationsClient);

        await Assert.ThrowsExactlyAsync<ApplicationNotFoundException>(()
            => controller.InvokeAsync(new GetApplicationApiConfigurationRequest {
                ClientId = "mock-client-id"
            }));
    }
}
