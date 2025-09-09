using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Access;
using Dfe.SignIn.NodeApi.Client.Access.Models;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Access;

[TestClass]
public sealed class GetApplicationsAssociatedWithUserNodeRequesterTests
{
    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetApplicationsAssociatedWithUserRequest,
            GetApplicationsAssociatedWithUserNodeRequester
        >();
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedOrganisation()
    {
        ApplicationDto[] mockDto = [
            new ApplicationDto() {
                AccessGrantedOn = DateTime.Parse("1980-01-01"),
                Identifiers = [],
                OrganisationId = Guid.Parse("43223fa8-1929-4670-859e-1788f383a45e"),
                Roles = [],
                ServiceId = Guid.Parse("5f550065-59f5-4e36-bfcf-5f6fbed31426"),
                UserId = Guid.Parse("8f79e542-7b8a-4904-b6fb-85b0fa41a530")
            },
            new ApplicationDto() {
                AccessGrantedOn = DateTime.Parse("1980-01-02"),
                Identifiers = [],
                OrganisationId = Guid.Parse("909073b4-f903-4d5a-9705-fa7e028b8cfd"),
                Roles = [],
                ServiceId = Guid.Parse("87588e6f-3b21-48b0-be42-b1239a673ed1"),
                UserId = Guid.Parse("8f79e542-7b8a-4904-b6fb-85b0fa41a530")
            }
        ];

        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(mockDto), System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var accessClient = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://access.localhost")
        };

        var controller = new GetApplicationsAssociatedWithUserNodeRequester(accessClient);

        var response = await controller.InvokeAsync(new GetApplicationsAssociatedWithUserRequest {
            UserId = mockDto[0].UserId
        });

        var applications = response.UserApplicationMappings.ToArray();

        Assert.AreEqual(applications[0], new UserApplicationMapping {
            AccessGranted = mockDto[0].AccessGrantedOn,
            ApplicationId = mockDto[0].ServiceId,
            OrganisationId = mockDto[0].OrganisationId,
            UserId = mockDto[0].UserId
        });

        Assert.AreEqual(applications[1], new UserApplicationMapping {
            AccessGranted = mockDto[1].AccessGrantedOn,
            ApplicationId = mockDto[1].ServiceId,
            OrganisationId = mockDto[1].OrganisationId,
            UserId = mockDto[1].UserId
        });
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsEmptyCollectionOfApplicationsWhenNotFound()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var accessClient = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://access.localhost")
        };

        var controller = new GetApplicationsAssociatedWithUserNodeRequester(accessClient);

        var response = await controller.InvokeAsync(new GetApplicationsAssociatedWithUserRequest {
            UserId = Guid.Parse("8f79e542-7b8a-4904-b6fb-85b0fa41a530")
        });

        Assert.AreEqual(response.UserApplicationMappings, []);
    }
}
