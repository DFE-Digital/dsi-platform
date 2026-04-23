using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Moq.AutoMock;
using Dfe.SignIn.Core.UseCases.UnitTests.TestHelpers;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public class GetServiceUsersUseCaseTests
{
    private static readonly Guid ServiceId = Guid.NewGuid();
    private const string ClientId = "test-client";

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetServiceUsersRequest,
            GetServiceUsersUseCase
        >();
    }

    private static async Task SetupFakeDataAsync(AutoMocker autoMocker)
    {
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();
        var dirCtx = autoMocker.UseInMemoryDirectoriesDb();

        orgCtx.Services.Add(new ServiceEntity { 
            Id = ServiceId, 
            ClientId = ClientId, 
            Name = "Test Service", 
            ClientSecret = "secret",
            IsExternalService = true,
            IsMigrated = true,
            IsChildService = false,
            IsIdOnlyService = false,
            IsHiddenService = false
        });

        var user1Id = Guid.NewGuid();
        var org1Id = Guid.NewGuid();

        orgCtx.Organisations.Add(new OrganisationEntity { 
            Id = org1Id, 
            Name = "Org 1",
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        orgCtx.UserServices.Add(new UserServiceEntity { 
            UserId = user1Id, 
            ServiceId = ServiceId, 
            OrganisationId = org1Id,
            CreatedAt = new DateTime(2023, 1, 1),
            Status = 1,
            UpdatedAt = new DateTime(2023, 1, 1)
        });

        dirCtx.Users.Add(new UserEntity { 
            Sub = user1Id, 
            Email = "user1@test.com", 
            FirstName = "User", 
            LastName = "One", 
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Password = "pass",
            Salt = "salt"
        });

        await orgCtx.SaveChangesAsync();
        await dirCtx.SaveChangesAsync();
    }

    [TestMethod]
    public async Task ReturnsPaginatedUsers_WithNoFilters()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDataAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();

        var response = await interactor.InvokeAsync(new GetServiceUsersRequest { 
            ClientId = ClientId, 
            PageNumber = 1, 
            PageSize = 25 
        });

        Assert.AreEqual(1, response.Users.Count);
        Assert.AreEqual("user1@test.com", response.Users[0].Email);
        Assert.AreEqual("Org 1", response.Users[0].Organisation?.Name);
    }

    [TestMethod]
    public async Task ReturnsEmptyList_WhenNoServiceUsersExist()
    {
        var autoMocker = new AutoMocker();
        var orgCtx = autoMocker.UseInMemoryOrganisationsDb();
        autoMocker.UseInMemoryDirectoriesDb();
        orgCtx.Services.Add(new ServiceEntity { 
            Id = ServiceId, 
            ClientId = ClientId, 
            Name = "Test Service", 
            ClientSecret = "secret",
            IsExternalService = true,
            IsMigrated = true,
            IsChildService = false,
            IsIdOnlyService = false,
            IsHiddenService = false
        });
        await orgCtx.SaveChangesAsync();

        var interactor = autoMocker.CreateInstance<GetServiceUsersUseCase>();

        var response = await interactor.InvokeAsync(new GetServiceUsersRequest { 
            ClientId = ClientId, 
            PageNumber = 1, 
            PageSize = 25 
        });

        Assert.AreEqual(0, response.Users.Count);
        Assert.AreEqual(0, response.NumberOfRecords);
    }
}
