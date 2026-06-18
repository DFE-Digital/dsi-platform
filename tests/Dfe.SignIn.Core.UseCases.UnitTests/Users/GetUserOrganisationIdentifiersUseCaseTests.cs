using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class GetUserOrganisationIdentifiersUseCaseTests
{
    private static readonly Guid UserId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001");
    private static readonly Guid OrganisationId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000002");

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserOrganisationIdentifiersRequest,
            GetUserOrganisationIdentifiersUseCase
        >();
    }

    private static async Task<DbOrganisationsContext> SetupFakeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbOrganisationsContext(options);

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            UserId = UserId,
            OrganisationId = OrganisationId,
            RoleId = 0,
            Status = 1,
            NumericIdentifier = 12345L,
            TextIdentifier = "ABC12",
        });

        await ctx.SaveChangesAsync();

        return ctx;
    }

    [TestMethod]
    public async Task ReturnsIdentifiers_WhenUserOrganisationExists()
    {
        var orgCtx = await SetupFakeDatabaseAsync();
        GetUserOrganisationIdentifiersUseCase useCase = new(orgCtx);

        var response = await useCase.InvokeAsync(
            new GetUserOrganisationIdentifiersRequest {
                UserId = UserId,
                OrganisationId = OrganisationId,
            }
        );

        Assert.AreEqual(12345L, response.NumericIdentifier);
        Assert.AreEqual("ABC12", response.TextIdentifier);
    }

    [TestMethod]
    public async Task ReturnsNullIdentifiers_WhenUserOrganisationDoesNotExist()
    {
        var orgCtx = await SetupFakeDatabaseAsync();
        GetUserOrganisationIdentifiersUseCase useCase = new(orgCtx);

        var response = await useCase.InvokeAsync(
            new GetUserOrganisationIdentifiersRequest {
                UserId = Guid.Parse("ffffffff-0000-0000-0000-000000000000"),
                OrganisationId = OrganisationId,
            }
        );

        Assert.IsNull(response.NumericIdentifier);
        Assert.IsNull(response.TextIdentifier);
    }

    [TestMethod]
    public async Task ReturnsNullIdentifiers_WhenNumericAndTextIdentifiersAreNotSet()
    {
        var autoMocker = new AutoMocker();
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var orgCtx = new DbOrganisationsContext(options);

        orgCtx.UserOrganisations.Add(new UserOrganisationEntity {
            UserId = UserId,
            OrganisationId = OrganisationId,
            RoleId = 0,
            Status = 1,
            NumericIdentifier = null,
            TextIdentifier = null,
        });
        await orgCtx.SaveChangesAsync();

        GetUserOrganisationIdentifiersUseCase useCase = new(orgCtx);

        var response = await useCase.InvokeAsync(
            new GetUserOrganisationIdentifiersRequest {
                UserId = UserId,
                OrganisationId = OrganisationId,
            }
        );

        Assert.IsNull(response.NumericIdentifier);
        Assert.IsNull(response.TextIdentifier);
    }
}
