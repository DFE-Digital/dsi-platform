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
            Id = Guid.Parse("b03e0aa5-f2a9-4926-8be0-9b996f97790d"),
            Name = "Fake Application A",
            Description = "The first fake application",
            ServiceHome = "https://fake-service-a.localhost",
            ClientId = "fake-app-a",
            ClientSecret = "fake-client-secret",
            ApiSecret = "ENC:0:encrypted-api-secret",
            IsExternalService = false,
            IsHiddenService = false,
            IsIdOnlyService = false,
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.Parse("4c32356b-15b3-462d-9e10-9d99f1a7194d"),
            Name = "Fake Application B",
            Description = "The second fake application",
            ServiceHome = "https://fake-service-b.localhost",
            ClientId = "fake-app-b",
            ClientSecret = "fake-client-secret",
            ApiSecret = "ENC:0:encrypted-api-secret",
            IsExternalService = true,
            IsHiddenService = true,
            IsIdOnlyService = true,
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.Parse("5fa54a1d-e751-428f-98fd-dc64e78decd1"),
            Name = "Fake Application C",
            Description = "The third fake application",
            ServiceHome = null,
            ClientId = "fake-app-c",
            ClientSecret = "fake-client-secret",
            ApiSecret = "ENC:0:encrypted-api-secret",
            IsExternalService = true,
            IsHiddenService = false,
            IsIdOnlyService = false,
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

    public static IEnumerable<object[]> GetCasesForReturnsExpectedInformation()
    {
        yield return new object[] {
            "fake-app-a",
            new Application {
                Id = Guid.Parse("b03e0aa5-f2a9-4926-8be0-9b996f97790d"),
                ClientId = "fake-app-a",
                Description = "The first fake application",
                Name = "Fake Application A",
                ServiceHomeUrl = new Uri("https://fake-service-a.localhost"),
                IsExternalService = false,
                IsHiddenService = false,
                IsIdOnlyService = false,
            },
        };
        yield return new object[] {
            "fake-app-b",
            new Application {
                Id = Guid.Parse("4c32356b-15b3-462d-9e10-9d99f1a7194d"),
                ClientId = "fake-app-b",
                Description = "The second fake application",
                Name = "Fake Application B",
                ServiceHomeUrl = new Uri("https://fake-service-b.localhost"),
                IsExternalService = true,
                IsHiddenService = true,
                IsIdOnlyService = true,
            },
        };
        yield return new object[] {
            "fake-app-c",
            new Application {
                Id = Guid.Parse("5fa54a1d-e751-428f-98fd-dc64e78decd1"),
                ClientId = "fake-app-c",
                Description = "The third fake application",
                Name = "Fake Application C",
                ServiceHomeUrl = null,
                IsExternalService = true,
                IsHiddenService = false,
                IsIdOnlyService = false,
            },
        };
    }

    [TestMethod]
    [DynamicData(nameof(GetCasesForReturnsExpectedInformation), DynamicDataSourceType.Method)]
    public async Task ReturnsExpectedInformation(string clientId, Application expectedApplication)
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<GetApplicationByClientIdUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationByClientIdRequest {
                ClientId = clientId,
            }
        );

        Assert.AreEqual(expectedApplication, response.Application);
    }
}
