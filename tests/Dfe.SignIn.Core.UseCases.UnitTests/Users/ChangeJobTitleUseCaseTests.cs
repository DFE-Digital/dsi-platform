using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class ChangeJobTitleUseCaseTests
{

    private static readonly Guid ExistingChangedUserId = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691");

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            ChangeJobTitleRequest,
            ChangeJobTitleUseCase
        >();
    }

    private static async Task<DbDirectoriesContext> SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryDirectoriesDb();

        ctx.Users.Add(new UserEntity {
            Sub = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691"),
            FirstName = "Alex",
            LastName = "Johnson",
            JobTitle = null,
            Email = "alex.johnson@example.com",
            Password = "",
            Salt = "",
        });

        ctx.Users.Add(new UserEntity {
            Sub = Guid.Parse("9e5979b7-3093-4e2b-a7bf-ebd3f5fc6d46"),
            FirstName = "Bob",
            LastName = "Johnson",
            JobTitle = "Unchanged",
            Email = "bob.johnson@example.com",
            Password = "",
            Salt = "",
        });

        await ctx.SaveChangesAsync();

        return ctx;
    }

    private static async Task<(ChangeJobTitleUseCase Interactor, DbDirectoriesContext Db, List<WriteToAuditRequest> Audit)>
    SetupAsync()
    {
        var autoMocker = new AutoMocker();
        var db = await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<ChangeJobTitleUseCase>();

        var capturedAudit = new List<WriteToAuditRequest>();
        autoMocker.CaptureRequest<WriteToAuditRequest>(capturedAudit.Add);

        return (interactor, db, capturedAudit);
    }

    [TestMethod]
    public async Task UserDoesNotExist_ThrowsUserNotFoundException()
    {
        var (interactor, _, _) = await SetupAsync();

        Guid nonExistentUserId = Guid.Parse("6d690a96-c392-4482-b750-733ea472bc96");

        var exception = await Assert.ThrowsExactlyAsync<UserNotFoundException>(()
            => interactor.InvokeAsync(
                new ChangeJobTitleRequest {
                    UserId = nonExistentUserId,
                    NewJobTitle = "Software Developer"
                }
            ));
        Assert.AreEqual(nonExistentUserId, exception.UserId);
    }

    [TestMethod]
    public async Task UserExists_ReturnsResponse()
    {
        var (interactor, _, _) = await SetupAsync();

        var response = await interactor.InvokeAsync(
            new ChangeJobTitleRequest {
                UserId = ExistingChangedUserId,
                NewJobTitle = "Software Developer"
            }
        );

        Assert.IsNotNull(response);
    }

    [TestMethod]
    public async Task UserExists_UpdatesJobTitle()
    {
        var (interactor, db, _) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeJobTitleRequest {
                UserId = ExistingChangedUserId,
                NewJobTitle = "Software Developer"
            }
        );

        var user = db.Users.Single(x => x.Sub == ExistingChangedUserId);
        Assert.AreEqual("Software Developer", user.JobTitle);
    }

    [TestMethod]
    public async Task UserExists_MatchingJobTitle_DoesNotDispatchAuditEvent()
    {
        var (interactor, _, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeJobTitleRequest {
                UserId = ExistingChangedUserId,
                NewJobTitle = null!
            }
        );

        Assert.IsEmpty(capturedAudit);
    }

    [TestMethod]
    public async Task UserExists_UpdatesJobTitle_ButNotOtherUsers()
    {
        var (interactor, db, _) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeJobTitleRequest {
                UserId = ExistingChangedUserId,
                NewJobTitle = "Software Developer"
            }
        );

        var user = db.Users.Single(x => x.Sub == Guid.Parse("9e5979b7-3093-4e2b-a7bf-ebd3f5fc6d46"));
        Assert.AreEqual("Unchanged", user.JobTitle);
    }

    [TestMethod]
    public async Task UserExists_DispatchesAuditEvent()
    {
        var (interactor, _, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeJobTitleRequest {
                UserId = ExistingChangedUserId,
                NewJobTitle = "Software Developer"
            }
        );

        Assert.HasCount(1, capturedAudit);

        var audit = capturedAudit.Single();
        Assert.AreEqual(AuditEventCategoryNames.ChangeJobTitle, audit.EventCategory);
        Assert.AreEqual("Successfully changed job title to Software Developer", audit.Message);
        Assert.AreEqual(ExistingChangedUserId, audit.UserId);
    }
}
