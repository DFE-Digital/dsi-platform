using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.UseCases.Organisations;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Organisations;

[TestClass]
public sealed class GetUsersAtOrganisationUseCaseTests
{
    private static readonly Guid OrganisationAId = Guid.NewGuid();
    private static readonly Guid OrganisationBId = Guid.NewGuid();

    private static readonly Guid ServiceId = Guid.NewGuid();

    private const string ClientId = "test-client";
    private const string Ukprn = "12345678";
    private const string Upin = "87654321";

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        // Organisations
        ctx.Organisations.Add(new OrganisationEntity {
            Id = OrganisationAId,
            Name = "Test Organisation A",
            Ukprn = Ukprn,
            Upin = null,
            Status = 1
        });

        ctx.Organisations.Add(new OrganisationEntity {
            Id = OrganisationBId,
            Name = "Test Organisation B",
            Ukprn = null,
            Upin = Upin,
            Status = 1
        });

        // Service
        ctx.Services.Add(new ServiceEntity {
            Id = ServiceId,
            ClientId = ClientId,
            Name = "Test Service",
            ClientSecret = "secret"
        });

        // Users
        var activeUser = new UserEntity {
            Sub = Guid.NewGuid(),
            Email = "user1@test.com",
            FirstName = "John",
            LastName = "Doe",
            Status = 1,
            Password = "pa55w0rd",
            Salt = "seasalt"
        };

        var inactiveUser = new UserEntity {
            Sub = Guid.NewGuid(),
            Email = "user2@test.com",
            FirstName = "Jane",
            LastName = "Smith",
            Status = 0,
            Password = "pwd",
            Salt = "pepper"
        };

        ctx.Users.AddRange(activeUser, inactiveUser);

        // User services
        ctx.UserServices.Add(new UserServiceEntity {
            Id = Guid.NewGuid(),
            OrganisationId = OrganisationAId,
            ServiceId = ServiceId,
            UserId = activeUser.Sub
        });

        ctx.UserServices.Add(new UserServiceEntity {
            Id = Guid.NewGuid(),
            OrganisationId = OrganisationAId,
            ServiceId = ServiceId,
            UserId = inactiveUser.Sub
        });

        // Roles
        var role = new RoleEntity {
            Id = Guid.NewGuid(),
            Code = "Admin",
            Name = "Administrator",
            ApplicationId = ServiceId
        };

        ctx.Roles.Add(role);

        // User roles
        ctx.UserServiceRoles.Add(new UserServiceRoleEntity {
            OrganisationId = OrganisationAId,
            ServiceId = ServiceId,
            UserId = activeUser.Sub,
            RoleId = role.Id
        });

        await ctx.SaveChangesAsync();
    }

    [TestMethod]
    public async Task ReturnsUsers_WhenMatchingUkprn()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var useCase = autoMocker.CreateInstance<GetUsersAtOrganisationUseCase>();

        var response = await useCase.InvokeAsync(
            new GetUsersAtOrganisationRequestRaw(ClientId, Ukprn)
        );

        Assert.IsTrue(response.IsUkprn);
        Assert.AreEqual(Ukprn, response.ExternalId);
        Assert.AreEqual(2, response?.Users?.Count());

        var user = response.Users.First(u => u.FirstName == "John");
        Assert.AreEqual("user1@test.com", user.Email);
        Assert.AreEqual("Admin", user.Role);
    }

    [TestMethod]
    public async Task FallsBackToUpin_WhenUkprnHasNoResults()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var useCase = autoMocker.CreateInstance<GetUsersAtOrganisationUseCase>();

        var response = await useCase.InvokeAsync(
            new GetUsersAtOrganisationRequestRaw(ClientId, Upin)
        );

        Assert.IsFalse(response.IsUkprn);
        Assert.AreEqual(Upin, response.ExternalId);
        Assert.AreEqual(0, response?.Users?.Count());
    }

    [TestMethod]
    public async Task ReturnsEmpty_WhenNoMatchingOrganisation()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var useCase = autoMocker.CreateInstance<GetUsersAtOrganisationUseCase>();

        var response = await useCase.InvokeAsync(
            new GetUsersAtOrganisationRequestRaw(ClientId, "99999999")
        );

        Assert.IsFalse(response.IsUkprn);
        Assert.AreEqual(0, response?.Users?.Count());
    }

    [TestMethod]
    public async Task FiltersUsers_ByClientId()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var useCase = autoMocker.CreateInstance<GetUsersAtOrganisationUseCase>();

        // Different clientId -> should return no users
        var response = await useCase.InvokeAsync(
            new GetUsersAtOrganisationRequestRaw("other-client", Ukprn)
        );

        Assert.AreEqual(0, response?.Users?.Count());
    }

    [TestMethod]
    public async Task ReturnsUsers_WithNullRole_WhenNoRoleAssigned()
    {
        var autoMocker = new AutoMocker();

        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        var orgId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        ctx.Organisations.Add(new OrganisationEntity { Id = orgId, Ukprn = Ukprn, Name = "New organisation", Status = 1 });

        ctx.Services.Add(new ServiceEntity {
            Id = serviceId,
            ClientId = ClientId,
            Name = "Service",
            ClientSecret = "topsecret"
        });

        var user = new UserEntity {
            Sub = Guid.NewGuid(),
            Email = "user@test.com",
            FirstName = "Test",
            LastName = "User",
            Status = 1,
            Password = "password123",
            Salt = "pinch"
        };

        ctx.Users.Add(user);

        ctx.UserServices.Add(new UserServiceEntity {
            OrganisationId = orgId,
            ServiceId = serviceId,
            UserId = user.Sub
        });

        await ctx.SaveChangesAsync();

        var useCase = autoMocker.CreateInstance<GetUsersAtOrganisationUseCase>();

        var response = await useCase.InvokeAsync(
            new GetUsersAtOrganisationRequestRaw(ClientId, Ukprn)
        );

        Assert.AreEqual(1, response?.Users?.Count());
        Assert.IsNull(response.Users.First().Role);
    }
}
