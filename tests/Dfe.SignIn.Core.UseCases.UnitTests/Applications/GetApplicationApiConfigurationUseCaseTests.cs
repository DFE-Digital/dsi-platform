using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.UseCases.Applications;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Applications;

[TestClass]
public sealed class GetApplicationApiConfigurationUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        var autoMocker = new AutoMocker();
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var orgCtx = new DbOrganisationsContext(options);
        autoMocker.Use(orgCtx);

        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetApplicationApiConfigurationRequest,
            GetApplicationApiConfigurationUseCase
        >(autoMocker);
    }

    private static async Task<DbOrganisationsContext> SetupFakeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbOrganisationsContext(options);

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.Parse("b03e0aa5-f2a9-4926-8be0-9b996f97790d"),
            Name = "Fake Application",
            ClientId = "fake-app",
            ClientSecret = "fake-client-secret",
            ApiSecret = "ENC:0:encrypted-api-secret",
        });

        await ctx.SaveChangesAsync();

        return ctx;
    }

    [TestMethod]
    public async Task Throws_WhenApplicationNotFound()
    {
        var orgCtx = await SetupFakeDatabaseAsync();
        GetApplicationApiConfigurationUseCase interactor = new(orgCtx, Mock.Of<IInteractionDispatcher>());

        string nonExistentClientId = "non-existent";

        var exception = await Assert.ThrowsExactlyAsync<ApplicationNotFoundException>(()
            => interactor.InvokeAsync(
                new GetApplicationApiConfigurationRequest {
                    ClientId = nonExistentClientId,
                }
            ));
        Assert.AreEqual(nonExistentClientId, exception.ClientId);
    }

    [TestMethod]
    public async Task ReturnsApiConfiguration()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = await SetupFakeDatabaseAsync();

        autoMocker.Use(orgCtx); // register DbContext

        autoMocker.MockResponse(
            new DecryptApiSecretRequest {
                EncryptedApiSecret = "ENC:0:encrypted-api-secret",
            },
            new DecryptApiSecretResponse {
                ApiSecret = "decrypted-api-secret",
            }
        );

        var interactor = autoMocker
            .CreateInstance<GetApplicationApiConfigurationUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationApiConfigurationRequest {
                ClientId = "fake-app",
            }
        );

        Assert.AreEqual("fake-app", response.Configuration.ClientId);
        Assert.AreEqual("decrypted-api-secret", response.Configuration.ApiSecret);
    }
}
