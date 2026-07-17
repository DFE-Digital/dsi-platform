using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class GetUserServiceAccessDetailsUseCaseTests
{
    private static readonly Guid UserId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001");
    private static readonly Guid ServiceId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000002");
    private static readonly Guid OrganisationId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000003");
    private static readonly Guid RoleId = Guid.Parse("b1b2b3b4-0000-0000-0000-000000000001");

    private static readonly GetUserServiceAccessDetailsRequest ValidRequest = new() {
        UserId = UserId,
        ServiceId = ServiceId,
        OrganisationId = OrganisationId,
    };

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
            GetUserServiceAccessDetailsRequest,
            GetUserServiceAccessDetailsUseCase
        >(autoMocker);
    }

    private static async Task<AutoMocker> SetupWithAccessAsync()
    {
        var autoMocker = new AutoMocker();
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbOrganisationsContext(options);

        // Seed UserService (the access record)
        ctx.UserServices.Add(new UserServiceEntity {
            Id = Guid.NewGuid(),
            UserId = UserId,
            ServiceId = ServiceId,
            OrganisationId = OrganisationId,
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });

        // Seed a Role
        ctx.Roles.Add(new RoleEntity {
            Id = RoleId,
            Name = "Role A",
            Code = "role-a",
            NumericId = 1,
            Status = 0,
        });

        // Seed UserServiceRole
        ctx.UserServiceRoles.Add(new UserServiceRoleEntity {
            Id = Guid.NewGuid(),
            UserId = UserId,
            ServiceId = ServiceId,
            OrganisationId = OrganisationId,
            RoleId = RoleId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });

        // Seed UserServiceIdentifier
        ctx.UserServiceIdentifiers.Add(new UserServiceIdentifierEntity {
            UserId = UserId,
            ServiceId = ServiceId,
            OrganisationId = OrganisationId,
            IdentifierKey = "externalId",
            IdentifierValue = "EXT-001",
        });

        // Seed UserOrganisation (for legacy identifiers)
        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            UserId = UserId,
            OrganisationId = OrganisationId,
            RoleId = 0,
            Status = 1,
            NumericIdentifier = 42L,
            TextIdentifier = "ABCDE",
        });

        // Seed Organisation (for legacy ID and APAR)
        ctx.Organisations.Add(new OrganisationEntity {
            Id = OrganisationId,
            Name = "Test School",
            Status = 1,
            LegacyId = 99L,
            IsOnApar = "true",
        });

        await ctx.SaveChangesAsync();

        // Still mock the two remaining interaction dispatches
        autoMocker.MockResponse<GetUserOrganisationIdentifiersRequest>(
            new GetUserOrganisationIdentifiersResponse {
                NumericIdentifier = 42L,
                TextIdentifier = "ABCDE",
            }
        );

        autoMocker.MockResponse<GetOrganisationByIdRequest>(
            new GetOrganisationByIdResponse {
                Organisation = new Organisation {
                    Id = OrganisationId,
                    Name = "Test School",
                    Status = OrganisationStatus.Open,
                    LegacyId = 99L,
                    IsOnApar = "true",
                }
            }
        );

        autoMocker.Use(ctx); // register DbContext

        return autoMocker;
    }

    [TestMethod]
    public async Task Throws_WhenUserHasNoAccess()
    {
        var autoMocker = new AutoMocker();

        // Simulate a scenario where the database is empty and there is no UserService row for the user.
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbOrganisationsContext(options);
        autoMocker.Use(ctx);

        var useCase = autoMocker.CreateInstance<GetUserServiceAccessDetailsUseCase>();

        await Assert.ThrowsExactlyAsync<UserServiceAccessNotFoundException>(()
            => useCase.InvokeAsync(ValidRequest));
    }

    [TestMethod]
    public async Task ReturnsExpectedIds()
    {
        var autoMocker = await SetupWithAccessAsync();
        var useCase = autoMocker.CreateInstance<GetUserServiceAccessDetailsUseCase>();

        var response = await useCase.InvokeAsync(ValidRequest);

        Assert.AreEqual(UserId, response.UserId);
        Assert.AreEqual(ServiceId, response.ServiceId);
        Assert.AreEqual(OrganisationId, response.OrganisationId);
    }

    [TestMethod]
    public async Task ReturnsRoles()
    {
        var autoMocker = await SetupWithAccessAsync();
        var useCase = autoMocker.CreateInstance<GetUserServiceAccessDetailsUseCase>();

        var response = await useCase.InvokeAsync(ValidRequest);

        var roles = response.Roles.ToArray();
        Assert.HasCount(1, roles);
        Assert.AreEqual("role-a", roles[0].Code);
    }

    [TestMethod]
    public async Task ReturnsIdentifiers()
    {
        var autoMocker = await SetupWithAccessAsync();
        var useCase = autoMocker.CreateInstance<GetUserServiceAccessDetailsUseCase>();

        var response = await useCase.InvokeAsync(ValidRequest);

        var identifiers = response.Identifiers.ToArray();
        Assert.HasCount(1, identifiers);
        Assert.AreEqual("externalId", identifiers[0].Key);
        Assert.AreEqual("EXT-001", identifiers[0].Value);
    }
}
