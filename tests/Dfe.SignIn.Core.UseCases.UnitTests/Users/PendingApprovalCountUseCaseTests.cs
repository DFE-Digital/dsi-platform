
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.UseCases.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class PendingApprovalCountUseCaseTests
{
    private static readonly Guid ApprovalUserId = Guid.Parse("6d690a96-c392-4482-b750-733ea472bc96");

    [TestMethod]
    public async Task GetPendingApprovalCount_Returns_OneWhenUserRequestAccessToOrganisation()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker, CreatePendingOrganisationRequest: true);

        var interactor = autoMocker.CreateInstance<PendingApprovalCountUseCase>();

        // Act
        var result = await interactor.InvokeAsync(new GetPendingApprovalCountRequest() { UserId = ApprovalUserId });

        // Assert
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetPendingApprovalCount_Returns_OneWhenUserRequestAccessToService()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker, CreatePendingOrganisationRequest: false, CreatePendingServiceRequest: true);

        var interactor = autoMocker.CreateInstance<PendingApprovalCountUseCase>();

        // Act
        var result = await interactor.InvokeAsync(new GetPendingApprovalCountRequest() { UserId = ApprovalUserId });

        // Assert
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetPendingApprovalCount_Returns_TwoWhenUserRequestsAccessToOrganisationAndService()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker, CreatePendingOrganisationRequest: true, CreatePendingServiceRequest: true);

        var interactor = autoMocker.CreateInstance<PendingApprovalCountUseCase>();

        // Act
        var result = await interactor.InvokeAsync(new GetPendingApprovalCountRequest() { UserId = ApprovalUserId });

        // Assert
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task GetPendingApprovalCount_Returns_ZeroWhenNoActiveRequests()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<PendingApprovalCountUseCase>();

        // Act
        var result = await interactor.InvokeAsync(new GetPendingApprovalCountRequest() { UserId = ApprovalUserId });

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker, bool CreatePendingOrganisationRequest = false,
        bool CreatePendingServiceRequest = false)
    {
        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        var standardUser = Guid.Parse("1d690a94-c392-4482-b750-711ea472bc96");

        ctx.Organisations.Add(new OrganisationEntity {
            Id = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
            Name = "Test Organisation 1",
            Status = (int)OrganisationStatus.Open,
            Category = "002",
        });

        // Add approver
        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            OrganisationId = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
            UserId = ApprovalUserId,
            RoleId = 10000,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = 1
        });

        ctx.Services.Add(new ServiceEntity {
            Id = Guid.Parse("d111bd11-01c1-1fcf-b0f1-1101bc1110f1"),
            Name = "My Test Service",
            Description = "testing",
            ClientId = "test",
            ClientSecret = "test"
        });

        if (CreatePendingOrganisationRequest) {
            ctx.UserOrganisationRequests.Add(new UserOrganisationRequestEntity {
                Id = Guid.Parse("e123bd61-06c5-4fcf-b0f1-7509bc3570f1"),
                CreatedAt = DateTime.Now,
                UserId = standardUser,
                OrganisationId = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4")
            });
        }

        if (CreatePendingServiceRequest) {
            ctx.UserServiceRequests.Add(new UserServiceRequestEntity {
                Id = Guid.Parse("e123bd61-06c5-4fcf-b0f1-7509bc3570f1"),
                CreatedAt = DateTime.Now,
                UserId = standardUser,
                ServiceId = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
                RequestType = "1",
                OrganisationId = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4")
            });
        }

        await ctx.SaveChangesAsync();

    }
}
