using System.Net;
using System.Text;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.NodeApi.Client.Applications;
using Dfe.SignIn.NodeApi.Client.Applications.Models;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;
using Moq;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Applications;

[TestClass]
public sealed class GetApplicationApiConfigurationNodeRequesterTests
{
    private Mock<IInteractionDispatcher> mockInteraction = null!;

    [TestInitialize]
    public void Setup()
    {
        this.mockInteraction = new Mock<IInteractionDispatcher>();

        this.mockInteraction
            .Setup(b => b.DispatchAsync(
                It.IsAny<InteractionContext<DecryptPublicApiSecretRequest>>()
            ))
            .Returns(InteractionTask.FromResult(new DecryptedPublicApiSecretResponse {
                ApiSecret = "mock-api-secret"
            }));
    }

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetApplicationApiConfigurationRequest,
            GetApplicationApiConfigurationNodeRequester
        >();
    }

    [TestMethod]
    public async Task ReturnsExpectedOrganisation()
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

        var controller = new GetApplicationApiConfigurationNodeRequester(this.mockInteraction.Object, applicationsClient);

        var response = await controller.InvokeAsync(new GetApplicationApiConfigurationRequest {
            ClientId = "mock-client-id"
        });

        Assert.IsNotNull(response.Configuration);

        Assert.AreEqual(response.Configuration, new ApplicationApiConfiguration {
            ClientId = mockDto.RelyingParty.ClientId,
            ApiSecret = mockDto.RelyingParty.ApiSecret,
        });
    }

    [TestMethod]
    public async Task Throws_WhenApplicationNotFound()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var applicationsClient = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://applications.localhost")
        };

        var controller = new GetApplicationApiConfigurationNodeRequester(this.mockInteraction.Object, applicationsClient);

        await Assert.ThrowsExactlyAsync<ApplicationNotFoundException>(()
            => controller.InvokeAsync(new GetApplicationApiConfigurationRequest {
                ClientId = "mock-client-id"
            }));
    }
}
