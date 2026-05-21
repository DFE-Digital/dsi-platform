
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.UseCases.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class IsOrganisationApproverUseCaseTests
{
    [TestMethod]
    public async Task Approver_IsFalseWhenUserHasNoApproverPermissions()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<IsOrganisationApproverUseCase>();

        Guid nonExistentUserId = Guid.Parse("6d690a96-c392-4482-b750-733ea472bc96");

        // Act
        var result = await interactor.InvokeAsync(new IsOrganisationApproverRequest(nonExistentUserId));

        // Assert
        Assert.IsFalse(result.IsApprover);
    }

    [TestMethod]
    public async Task Approver_IsTrueWhenUserHasApproverPermissions()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<IsOrganisationApproverUseCase>();

        Guid nonExistentUserId = Guid.Parse("1d690a94-c392-4482-b750-711ea472bc96");

        // Act
        var result = await interactor.InvokeAsync(new IsOrganisationApproverRequest(nonExistentUserId));

        // Assert
        Assert.IsTrue(result.IsApprover);
    }

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        var user1Id = Guid.Parse("6d690a96-c392-4482-b750-733ea472bc96");
        var user2Id = Guid.Parse("1d690a94-c392-4482-b750-711ea472bc96");
        ctx.Organisations.Add(new OrganisationEntity {
            Id = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
            Name = "Test Organisation 1",
            Status = (int)OrganisationStatus.Open,
            Category = "002",
        });

        // Add approver
        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            OrganisationId = Guid.Parse("1d690a94-c392-4482-b750-711ea472bc96"),
            UserId = user2Id,
            RoleId = 10000,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = 1
        });

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            OrganisationId = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
            UserId = user1Id,
            RoleId = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = 1
        });

        ctx.Organisations.Add(new OrganisationEntity {
            Id = Guid.Parse("f9fb271e-2f7c-439c-9c69-332d3281b2d1"),
            Name = "Test Organisation 2",
            Status = (int)OrganisationStatus.Closed,
            Category = "002",
        });

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            OrganisationId = Guid.Parse("f9fb271e-2f7c-439c-9c69-332d3281b2d1"),
            UserId = user1Id,
            RoleId = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = 1
        });

        ctx.Organisations.Add(new OrganisationEntity {
            Id = Guid.Parse("bc4ab66f-c33a-4aa0-b602-4d40b66f6923"),
            Name = "Not Found",
            Status = (int)OrganisationStatus.Closed,
            Category = "002",
        });

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            OrganisationId = Guid.Parse("bc4ab66f-c33a-4aa0-b602-4d40b66f6923"),
            UserId = Guid.Parse("5ea4c9c8-e737-41aa-8d70-87328f7bfd26"),
            RoleId = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = 1
        });

        await ctx.SaveChangesAsync();

    }
}
