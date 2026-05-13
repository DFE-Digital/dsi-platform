using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class ChangeNameUseCaseTests
{
    private static readonly Guid ExistingChangedUserId = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691");
    private static async Task<(ChangeNameUseCase Interactor, DbDirectoriesContext Db, List<WriteToAuditRequest> Audit)>

    SetupAsync()
    {
        var autoMocker = new AutoMocker();
        var db = await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<ChangeNameUseCase>();

        var capturedAudit = new List<WriteToAuditRequest>();
        autoMocker.CaptureRequest<WriteToAuditRequest>(capturedAudit.Add);

        return (interactor, db, capturedAudit);
    }

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            ChangeNameRequest,
            ChangeNameUseCase
        >();
    }

    [TestMethod]
    public async Task UserDoesNotExist_ThrowsUserNotFoundException()
    {
        var (interactor, _, _) = await SetupAsync();

        Guid nonExistentUserId = Guid.Parse("6d690a96-c392-4482-b750-733ea472bc96");

        var exception = await Assert.ThrowsExactlyAsync<UserNotFoundException>(()
            => interactor.InvokeAsync(
                new ChangeNameRequest {
                    UserId = nonExistentUserId,
                    FirstName = "Firstname",
                    LastName = "LastName"
                }
            ));
        Assert.AreEqual(nonExistentUserId, exception.UserId);
    }

    [TestMethod]
    public async Task UserExists_UpdatesName()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeNameRequest {
                UserId = ExistingChangedUserId,
                FirstName = "Bob",
                LastName = "Test"
            }
        );

        var user = db.Users.Single(x => x.Sub == ExistingChangedUserId);

        Assert.AreEqual("Bob", user.FirstName);
        Assert.AreEqual("Test", user.LastName);

        Assert.IsTrue(capturedAudit.Count == 1);
        Assert.AreEqual(capturedAudit.First().Message, "Successfully changed users name to Bob Test");
    }

    [TestMethod]
    public async Task UserExists_UpdatesFirstName()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeNameRequest {
                UserId = ExistingChangedUserId,
                FirstName = "Bob",
                LastName = "Johnson"
            }
        );

        var user = db.Users.Single(x => x.Sub == ExistingChangedUserId);
        Assert.AreEqual("Bob", user.FirstName);
        Assert.AreEqual("Johnson", user.LastName);

        Assert.IsTrue(capturedAudit.Count == 1);
        Assert.AreEqual(capturedAudit.First().Message, "Successfully changed users name to Bob Johnson");
    }

    [TestMethod]
    public async Task UserExists_UpdatesLastName()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeNameRequest {
                UserId = ExistingChangedUserId,
                FirstName = "Alex",
                LastName = "Test"
            }
        );

        var user = db.Users.Single(x => x.Sub == ExistingChangedUserId);
        Assert.AreEqual("Alex", user.FirstName);
        Assert.AreEqual("Test", user.LastName);

        Assert.IsTrue(capturedAudit.Count == 1);
        Assert.AreEqual(capturedAudit.First().Message, "Successfully changed users name to Alex Test");
    }

    [TestMethod]
    public async Task UserExists_UpdatesLastNameWithNormalisedSpaces()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeNameRequest {
                UserId = ExistingChangedUserId,
                FirstName = "Alex",
                LastName = "Test     "
            }
        );

        var user = db.Users.Single(x => x.Sub == ExistingChangedUserId);
        Assert.AreEqual("Alex", user.FirstName);
        Assert.AreEqual("Test", user.LastName);

        Assert.IsTrue(capturedAudit.Count == 1);
        Assert.AreEqual(capturedAudit.First().Message, "Successfully changed users name to Alex Test");
    }

    [TestMethod]
    public async Task UserExists_UpdatesFirstNameWithNormalisedSpaces()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeNameRequest {
                UserId = ExistingChangedUserId,
                FirstName = "Alex     ",
                LastName = "Test"
            }
        );

        var user = db.Users.Single(x => x.Sub == ExistingChangedUserId);
        Assert.AreEqual("Alex", user.FirstName);
        Assert.AreEqual("Test", user.LastName);

        Assert.IsTrue(capturedAudit.Count == 1);
        Assert.AreEqual(capturedAudit.First().Message, "Successfully changed users name to Alex Test");
    }

    [TestMethod]
    public async Task UserExists_UpdatesMultipleFirstNameWithNormalisedSpaces()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeNameRequest {
                UserId = ExistingChangedUserId,
                FirstName = "Alex  Tester",
                LastName = "Test"
            }
        );

        var user = db.Users.Single(x => x.Sub == ExistingChangedUserId);
        Assert.AreEqual("Alex Tester", user.FirstName);
        Assert.AreEqual("Test", user.LastName);

        Assert.IsTrue(capturedAudit.Count == 1);
        Assert.AreEqual(capturedAudit.First().Message, "Successfully changed users name to Alex Tester Test");
    }


    [TestMethod]
    public async Task UserExists_UpdatesMultipleSurnameNameWithNormalisedSpaces()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
            new ChangeNameRequest {
                UserId = ExistingChangedUserId,
                FirstName = "Alex",
                LastName = "Test   Test"
            }
        );

        var user = db.Users.Single(x => x.Sub == ExistingChangedUserId);
        Assert.AreEqual("Alex", user.FirstName);
        Assert.AreEqual("Test Test", user.LastName);

        Assert.IsTrue(capturedAudit.Count == 1);
        Assert.AreEqual(capturedAudit.First().Message, "Successfully changed users name to Alex Test Test");
    }

    [TestMethod]
    public async Task UserExists_MatchingName_DoesNotDispatchAuditEvent()
    {
        var (interactor, _, capturedAudit) = await SetupAsync();

        await interactor.InvokeAsync(
           new ChangeNameRequest {
               UserId = ExistingChangedUserId,
               FirstName = "Alex",
               LastName = "Johnson"
           }
       );

        Assert.IsEmpty(capturedAudit);
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
}
