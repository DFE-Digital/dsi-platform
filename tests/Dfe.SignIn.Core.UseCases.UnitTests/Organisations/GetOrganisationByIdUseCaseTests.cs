using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.UseCases.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Organisations;

[TestClass]
public sealed class GetOrganisationByIdUseCaseTests
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
            GetOrganisationByIdRequest,
            GetOrganisationByIdUseCase
        >(autoMocker);
    }

    private static async Task<DbOrganisationsContext> SetupFakeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbOrganisationsContext(options);

        ctx.Organisations.Add(new OrganisationEntity {
            Id = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
            Name = "Test Organisation",
            Status = 1,
            Category = "002",
        });

        await ctx.SaveChangesAsync();

        return ctx;
    }

    [TestMethod]
    public async Task Throws_WhenOrganisationNotFound()
    {
        var orgCtx = await SetupFakeDatabaseAsync();
        GetOrganisationByIdUseCase interactor = new(orgCtx);

        Guid nonExistentUserId = Guid.Parse("6d690a96-c392-4482-b750-733ea472bc96");

        var exception = await Assert.ThrowsExactlyAsync<OrganisationNotFoundException>(()
            => interactor.InvokeAsync(
                new GetOrganisationByIdRequest {
                    OrganisationId = nonExistentUserId,
                }
            ));
        Assert.AreEqual(nonExistentUserId, exception.OrganisationId);
    }

    [TestMethod]
    public async Task ReturnsExpectedProfile()
    {
        var orgCtx = await SetupFakeDatabaseAsync();
        GetOrganisationByIdUseCase interactor = new(orgCtx);

        var response = await interactor.InvokeAsync(
            new GetOrganisationByIdRequest {
                OrganisationId = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
            }
        );

        var expectedResponse = new GetOrganisationByIdResponse {
            Organisation = new Organisation {
                Id = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
                Name = "Test Organisation",
                Status = OrganisationStatus.Open,
                Category = OrganisationCategory.LocalAuthority,
            }
        };

        Assert.AreEqual(expectedResponse, response);
    }
}
