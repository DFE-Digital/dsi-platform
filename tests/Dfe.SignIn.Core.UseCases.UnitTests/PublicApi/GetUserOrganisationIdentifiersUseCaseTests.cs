using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.UseCases.PublicApi;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.PublicApi;

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

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            UserId = UserId,
            OrganisationId = OrganisationId,
            RoleId = 0,
            Status = 1,
            NumericIdentifier = 12345L,
            TextIdentifier = "ABC12",
        });

        await ctx.SaveChangesAsync();
    }

    [TestMethod]
    public async Task ReturnsIdentifiers_WhenUserOrganisationExists()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var useCase = autoMocker.CreateInstance<GetUserOrganisationIdentifiersUseCase>();

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
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var useCase = autoMocker.CreateInstance<GetUserOrganisationIdentifiersUseCase>();

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
        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            UserId = UserId,
            OrganisationId = OrganisationId,
            RoleId = 0,
            Status = 1,
            NumericIdentifier = null,
            TextIdentifier = null,
        });
        await ctx.SaveChangesAsync();

        var useCase = autoMocker.CreateInstance<GetUserOrganisationIdentifiersUseCase>();

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
