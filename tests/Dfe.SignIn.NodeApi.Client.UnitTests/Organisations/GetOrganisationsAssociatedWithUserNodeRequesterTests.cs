using System.Net;
using System.Text;
using System.Text.Json;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.NodeApi.Client.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations.Models;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Organisations;

[TestClass]
public sealed class GetOrganisationsAssociatedWithUserNodeRequesterTests
{
    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetOrganisationsAssociatedWithUserRequest,
            GetOrganisationsAssociatedWithUserNodeRequester
        >();
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedOrganisation()
    {
        List<OrganisationsAssociatedWithUserDto> mockDtos = [];
        mockDtos.Add(
            new OrganisationsAssociatedWithUserDto {
                Organisation = new UserOrganisationDto {
                    Id = Guid.Parse("6bdc882b-4220-468e-bfc7-c41800fc0dc0"),
                    Name = "mock-name-1",
                    Status = new StatusDto {
                        Id = 1,
                        Name = "mock-name-1",
                        TagColor = "mock-tag-color-1",
                    },
                    Category = new CategoryDto {
                        Id = OrganisationConstants.CategoryId_Establishment,
                        Name = "Establishment",
                    },
                    EstablishmentType = new EstablishmentTypeDto {
                        Id = "29",
                        Name = "Higher education institution",
                    },
                }
            }
        );
        mockDtos.Add(
            new OrganisationsAssociatedWithUserDto {
                Organisation = new UserOrganisationDto {
                    Id = Guid.Parse("7ba6c812-b9b1-47b6-8239-ea35ca8746f8"),
                    Name = "mock-name-2",
                    Status = new StatusDto {
                        Id = 1,
                        Name = "mock-name-2",
                        TagColor = "mock-tag-color-2"
                    },
                    Category = new CategoryDto {
                        Id = "002",
                        Name = "Local Authority",
                    },
                }
            }
        );

        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(JsonSerializer.Serialize(mockDtos), Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://mock.localhost")
        };

        var controller = new GetOrganisationsAssociatedWithUserNodeRequester(client);

        var response = await controller.InvokeAsync(new GetOrganisationsAssociatedWithUserRequest {
            UserId = Guid.Parse("3a939152-d229-4ac2-9ffa-61cd85576f0e")
        });

        var organisations = response.Organisations.ToArray();

        Assert.AreEqual(organisations[0], new Organisation {
            Id = mockDtos[0].Organisation.Id,
            Name = mockDtos[0].Organisation.Name,
            Status = OrganisationStatus.Open,
            Category = OrganisationCategory.Establishment,
            EstablishmentType = EstablishmentType.HigherEducationInstitution,
        });

        Assert.AreEqual(organisations[1], new Organisation {
            Id = mockDtos[1].Organisation.Id,
            Name = mockDtos[1].Organisation.Name,
            Status = OrganisationStatus.Open,
            Category = OrganisationCategory.LocalAuthority,
        });
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsEmptyCollectionOfOrganisationsWhenNotFound()
    {
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            return Task.FromResult(response);
        });

        var client = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://mock.localhost")
        };

        var controller = new GetOrganisationsAssociatedWithUserNodeRequester(client);

        var response = await controller.InvokeAsync(new GetOrganisationsAssociatedWithUserRequest {
            UserId = Guid.Parse("3a939152-d229-4ac2-9ffa-61cd85576f0e")
        });

        Assert.AreEqual(response.Organisations, []);
    }
}
