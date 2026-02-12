using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.UseCases.Applications;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Applications;

[TestClass]
public sealed class GetApplicationByClientIdUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetApplicationByClientIdRequest,
            GetApplicationByClientIdUseCase
        >();
    }

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.Parse("01e876a1-a06d-4945-bbe0-599a3d73dd11"),
            Name = "Fake Application A",
            ClientId = "fake-app-a",
            ClientSecret = "fake-client-secret-a",
            Description = "The first fake example application.",
            ServiceHome = "https://fake-application-a.localhost",
            IsExternalService = false,
            IsHiddenService = false,
            IsIdOnlyService = false,
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.Parse("33510512-33f3-491c-86ca-e036726584d0"),
            Name = "Fake Application B",
            ClientId = "fake-app-b",
            ClientSecret = "fake-client-secret-b",
            Description = "The second fake example application.",
            ServiceHome = "https://fake-application-b.localhost",
            IsExternalService = true,
            IsHiddenService = true,
            IsIdOnlyService = true,
        });

        await ctx.SaveChangesAsync();
    }

    [TestMethod]
    public async Task Throws_WhenApplicationNotFound()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetApplicationByClientIdUseCase>();

        string nonExistentClientId = "non-existent";

        var exception = await Assert.ThrowsExactlyAsync<ApplicationNotFoundException>(()
            => interactor.InvokeAsync(
                new GetApplicationByClientIdRequest {
                    ClientId = nonExistentClientId,
                }
            ));
        Assert.AreEqual(nonExistentClientId, exception.ClientId);
    }

    public static IEnumerable<object[]> GetCasesForReturnsApplicationInformation()
    {
        yield return new object[] {
            "fake-app-a",
            new GetApplicationByClientIdResponse {
                Application = new() {
                    Id = Guid.Parse("01e876a1-a06d-4945-bbe0-599a3d73dd11"),
                    ClientId = "fake-app-a",
                    Description = "The first fake example application.",
                    Name = "Fake Application A",
                    ServiceHomeUrl = new Uri("https://fake-application-a.localhost"),
                    IsExternalService = false,
                    IsHiddenService = false,
                    IsIdOnlyService = false,
                }
            }
        };
        yield return new object[] {
            "fake-app-b",
            new GetApplicationByClientIdResponse {
                Application = new() {
                    Id = Guid.Parse("33510512-33f3-491c-86ca-e036726584d0"),
                    ClientId = "fake-app-b",
                    Description = "The second fake example application.",
                    Name = "Fake Application B",
                    ServiceHomeUrl = new Uri("https://fake-application-b.localhost"),
                    IsExternalService = true,
                    IsHiddenService = true,
                    IsIdOnlyService = true,
                }
            }
        };
    }

    [TestMethod]
    [DynamicData(nameof(GetCasesForReturnsApplicationInformation), DynamicDataSourceType.Method)]
    public async Task ReturnsApplicationInformation(string clientId, GetApplicationByClientIdResponse expectedResponse)
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<GetApplicationByClientIdUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationByClientIdRequest {
                ClientId = clientId,
            }
        );

        Assert.AreEqual(expectedResponse.Application, response.Application);
    }
}
