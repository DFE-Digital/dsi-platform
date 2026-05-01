
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public class GetServiceUsersUseCaseTests
{
    private static readonly Guid ServiceId = Guid.Parse("b1e1e1e1-1111-1111-1111-111111111111");
    private static readonly Guid OrgId = Guid.Parse("a1a1a1a1-2222-2222-2222-222222222222");
    private static readonly Guid UserId = Guid.Parse("c1c1c1c1-3333-3333-3333-333333333333");

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid() => InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetServiceUsersRequest,
            GetServiceUsersUseCase
        >();

    private static OrganisationEntity CreateOrganisation(string name = "Org 1")
        => new()
        {
            Id = OrgId,
            Name = name,
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static UserEntity CreateUser(
        Guid? userId = null,
        string email = "user1@test.com",
        short status = 1,
        DateTime? updatedAt = null)
        => new()
        {
            Sub = userId ?? UserId,
            Email = email,
            FirstName = "User",
            LastName = "One",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow,
            Password = "pass",
            Salt = "salt"
        };

    private static UserServiceEntity CreateUserService(Guid? userId = null, DateTime? createdAt = null)
        => new()
        {
            UserId = userId ?? UserId,
            ServiceId = ServiceId,
            OrganisationId = OrgId,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Status = 1,
            UpdatedAt = createdAt ?? DateTime.UtcNow
        };

    private static RoleEntity CreateRole(Guid roleId, string name = "Test Role")
        => new()
        {
            Id = roleId,
            Name = name,
            Code = "TEST",
            NumericId = 1,
            Status = 1,
            ApplicationId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static UserServiceRoleEntity CreateUserServiceRole(Guid roleId, RoleEntity? role = null)
    {
        var entity = new UserServiceRoleEntity
        {
            UserId = UserId,
            OrganisationId = OrgId,
            ServiceId = ServiceId,
            RoleId = roleId,
        };

        if (role != null)
        {
            entity.Role = role;
        }

        return entity;
    }

    private static GetServiceUsersRequest CreateRequest(
        int pageNumber = 1,
        int pageSize = 25,
        int? userStatus = null,
        DateTimeOffset? dateFrom = null,
        DateTimeOffset? dateTo = null)
        => new()
        {
            ApplicationId = ServiceId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            UserStatus = userStatus,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

    private static void AddServiceUser(
        DbOrganisationsContext orgCtx,
        Guid userId,
        string email,
        short status,
        DateTime updatedAt)
    {
        orgCtx.Users.Add(CreateUser(userId, email, status, updatedAt));
        orgCtx.UserServices.Add(new UserServiceEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ServiceId = ServiceId,
            OrganisationId = OrgId,
            CreatedAt = DateTime.UtcNow,
            Status = 1,
            UpdatedAt = DateTime.UtcNow
        });
    }

    [TestMethod]
    public async Task ReturnsEmpty_WhenNoServiceUsers()
    {
        var autoMocker = new AutoMocker();
        autoMocker.UseInMemoryOrganisationsDb();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(new GetServiceUsersRequest
        {
            ApplicationId = ServiceId,
            PageNumber = 1,
            PageSize = 25
        });

        Assert.AreEqual(0, response.Users.Count);
        Assert.AreEqual(0, response.NumberOfRecords);
        Assert.AreEqual(1, response.Page);
        Assert.AreEqual(0, response.NumberOfPages);
    }

    [TestMethod]
    public async Task ReturnsSingleUser_WithOrgAndRoles()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();

        orgCtx.Organisations.Add(CreateOrganisation());
        orgCtx.Users.Add(CreateUser());
        orgCtx.UserServices.Add(CreateUserService());

        var roleId = Guid.NewGuid();
        var role = CreateRole(roleId);

        orgCtx.UserServiceRoles.Add(CreateUserServiceRole(roleId, role));

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest());

        Assert.AreEqual(1, response.Users.Count);

        var user = response.Users[0];
        Assert.AreEqual("user1@test.com", user.Email);
        Assert.AreEqual("Org 1", user.Organisation?.Name);
        Assert.AreEqual("Test Role", user.Roles?[0].Name);
        Assert.AreEqual(1, user.Roles?.Count);
        Assert.AreEqual(1, response.NumberOfRecords);
        Assert.AreEqual(1, response.NumberOfPages);
    }

    [TestMethod]
    public async Task Handles_NullRoleForeignKey()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();

        orgCtx.Organisations.Add(CreateOrganisation());
        orgCtx.Users.Add(CreateUser());
        orgCtx.UserServices.Add(CreateUserService());

        var roleId = Guid.NewGuid();
        orgCtx.UserServiceRoles.Add(CreateUserServiceRole(roleId));

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest());

        Assert.AreEqual(1, response.Users.Count);
        Assert.AreEqual(0, response.Users[0].Roles?.Count);
    }

    [TestMethod]
    public async Task Handles_MissingUserOrganisationRole()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();

        orgCtx.Organisations.Add(CreateOrganisation("Test Org"));
        orgCtx.Users.Add(CreateUser());
        orgCtx.UserServices.Add(CreateUserService());

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest());

        Assert.AreEqual(1, response.Users.Count);

        var user = response.Users[0];
        Assert.AreEqual(string.Empty, user.RoleName);
        Assert.IsNull(user.RoleId);
    }

    [TestMethod]
    public async Task Pagination_SkipTake_WithCorrectOrdering()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();

        for (int i = 0; i < 3; i++)
        {
            var userId = Guid.Parse($"00000000-0000-0000-0000-00000000000{i + 1}");
            var user = CreateUser(userId, $"user{i + 1}@test.com");
            user.FirstName = $"User{i + 1}";
            user.LastName = "Test";

            orgCtx.Users.Add(user);
            orgCtx.UserServices.Add(new UserServiceEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ServiceId = ServiceId,
                OrganisationId = OrgId,
                CreatedAt = DateTime.UtcNow,
                Status = 1,
                UpdatedAt = DateTime.UtcNow
            });
        }

        orgCtx.Organisations.Add(CreateOrganisation());
        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest(2, 2));

        Assert.AreEqual(1, response.Users.Count);
        Assert.AreEqual(3, response.NumberOfRecords);
        Assert.AreEqual(2, response.NumberOfPages);
    }

    [TestMethod]
    public async Task ReturnsEmpty_WhenAllMappedUsersHaveEmptyEmail()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();

        orgCtx.Organisations.Add(CreateOrganisation());
        orgCtx.UserServices.Add(CreateUserService());
        orgCtx.Users.Add(CreateUser(email: string.Empty));

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest());

        Assert.AreEqual(0, response.Users.Count);
        Assert.AreEqual(0, response.NumberOfRecords);
        Assert.AreEqual(1, response.Page);
        Assert.AreEqual(0, response.NumberOfPages);
    }

    [TestMethod]
    public async Task Filters_ByUserStatus()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();
        orgCtx.Organisations.Add(CreateOrganisation());

        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000001"), "active1@test.com", 1, DateTime.UtcNow.AddDays(-1));
        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000002"), "inactive@test.com", 0, DateTime.UtcNow.AddDays(-1));
        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000003"), "active2@test.com", 1, DateTime.UtcNow.AddDays(-1));

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest(userStatus: 1));

        Assert.AreEqual(2, response.Users.Count);
        Assert.AreEqual(2, response.NumberOfRecords);
        Assert.AreEqual(1, response.NumberOfPages);
        Assert.IsTrue(response.Users.All(x => x.UserStatus == 1));
    }

    [TestMethod]
    public async Task Filters_ByDateFrom_Inclusive()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();
        orgCtx.Organisations.Add(CreateOrganisation());

        var jan01 = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var jan10 = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var jan20 = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc);

        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000011"), "user1@test.com", 1, jan01);
        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000012"), "user2@test.com", 1, jan10);
        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000013"), "user3@test.com", 1, jan20);

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest(dateFrom: new DateTimeOffset(jan10)));

        Assert.AreEqual(2, response.Users.Count);
        Assert.AreEqual(2, response.NumberOfRecords);
        Assert.IsTrue(response.Users.All(x => x.UpdatedAt >= jan10));
    }

    [TestMethod]
    public async Task Filters_ByDateTo_Inclusive()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();
        orgCtx.Organisations.Add(CreateOrganisation());

        var jan01 = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var jan10 = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var jan20 = new DateTime(2024, 1, 20, 0, 0, 0, DateTimeKind.Utc);

        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000021"), "user1@test.com", 1, jan01);
        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000022"), "user2@test.com", 1, jan10);
        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000023"), "user3@test.com", 1, jan20);

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest(dateTo: new DateTimeOffset(jan10)));

        Assert.AreEqual(2, response.Users.Count);
        Assert.AreEqual(2, response.NumberOfRecords);
        Assert.IsTrue(response.Users.All(x => x.UpdatedAt <= jan10));
    }

    [TestMethod]
    public async Task Filters_ByStatusAndDateRange()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();
        orgCtx.Organisations.Add(CreateOrganisation());

        var jan05 = new DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc);
        var jan10 = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var jan15 = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000031"), "match@test.com", 1, jan10);
        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000032"), "wrong-status@test.com", 0, jan10);
        AddServiceUser(orgCtx, Guid.Parse("00000000-0000-0000-0000-000000000033"), "out-of-range@test.com", 1, jan15.AddDays(10));

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest(
            userStatus: 1,
            dateFrom: new DateTimeOffset(jan05),
            dateTo: new DateTimeOffset(jan15)));

        Assert.AreEqual(1, response.Users.Count);
        Assert.AreEqual("match@test.com", response.Users[0].Email);
        Assert.AreEqual(1, response.NumberOfRecords);
        Assert.AreEqual(1, response.NumberOfPages);
    }

    [TestMethod]
    public async Task Pagination_AppliesAfterFiltering()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();
        orgCtx.Organisations.Add(CreateOrganisation());

        for (int i = 1; i <= 6; i++)
        {
            AddServiceUser(
                orgCtx,
                Guid.Parse($"00000000-0000-0000-0000-0000000001{i:00}"),
                $"active{i}@test.com",
                1,
                new DateTime(2024, 1, i, 0, 0, 0, DateTimeKind.Utc)
            );
        }

        for (int i = 1; i <= 2; i++)
        {
            AddServiceUser(
                orgCtx,
                Guid.Parse($"00000000-0000-0000-0000-0000000002{i:00}"),
                $"inactive{i}@test.com",
                0,
                new DateTime(2024, 2, i, 0, 0, 0, DateTimeKind.Utc)
            );
        }

        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();
        var response = await interactor.InvokeAsync(CreateRequest(pageNumber: 2, pageSize: 2, userStatus: 1));

        Assert.AreEqual(2, response.Users.Count);
        Assert.AreEqual(6, response.NumberOfRecords);
        Assert.AreEqual(3, response.NumberOfPages);
        Assert.AreEqual(2, response.Page);
        Assert.IsTrue(response.Users.All(x => x.UserStatus == 1));
    }
}
