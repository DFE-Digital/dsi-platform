using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class LinkEntraUserToDsiUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            LinkEntraUserToDsiRequest,
            LinkEntraUserToDsiUseCase
        >();
    }

    private static readonly Guid UserIdMatchingDsiUserId = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691");
    private static readonly Guid UserIdMatchingEntraOid = Guid.Parse("74f539fa-5de2-4b15-b983-24ca9612b7cb");
    private static readonly Guid UserEntraOid = Guid.Parse("ecf31b2d-03e8-4f00-9035-32d2dd4a9ed3");
    private static readonly Guid EntraOid = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8693");

    private static async Task<DbDirectoriesContext> SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {

        var ctx = autoMocker.UseInMemoryDirectoriesDb();
        ctx.Users.AddRange(new UserEntity {
            Sub = UserIdMatchingDsiUserId,
            IsEntra = false,
            FirstName = "Alex",
            LastName = "Johnson",
            Email = "alex.johnson@example.com",
            Password = "",
            Salt = "",
            Status = 1,
            UpdatedAt = DateTime.Now
        },
            new UserEntity {
                Sub = UserIdMatchingEntraOid,
                IsEntra = true,
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@example.com",
                EntraOid = UserEntraOid,
                Password = "",
                Salt = "",
                Status = 1,
                EntraLinked = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = DateTime.Now
            });

        await ctx.SaveChangesAsync();
        return ctx;
    }

    private static async Task<(LinkEntraUserToDsiUseCase Interactor, DbDirectoriesContext Db, List<WriteToAuditRequest> Audit)>
        SetupAsync()
    {
        var autoMocker = new AutoMocker();

        autoMocker.Use<TimeProvider>(
        new MockTimeProvider(new DateTimeOffset(2025, 11, 18, 17, 56, 45, TimeSpan.Zero)));
        var db = await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<LinkEntraUserToDsiUseCase>();

        var capturedAudit = new List<WriteToAuditRequest>();
        autoMocker.CaptureRequest<WriteToAuditRequest>(capturedAudit.Add);

        return (interactor, db, capturedAudit);
    }

    [TestMethod]
    public async Task Throws_WhenUserDoesNotExistByDsiUserId()
    {
        var (interactor, _, _) = await SetupAsync();
        Guid nonExistentUserId = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8699");

        var exception = await Assert.ThrowsExactlyAsync<UserNotFoundException>(async () => {
            await interactor.InvokeAsync(
                new LinkEntraUserToDsiRequest {
                    DsiUserId = nonExistentUserId,
                    EntraUserId = UserEntraOid,
                    FirstName = "Jane",
                    LastName = "Doe"
                });
        });

        Assert.AreEqual(nonExistentUserId, exception.UserId);
    }

    [TestMethod]
    public async Task Throws_WhenEntraAccountAlreadyLinkedToDifferentUser()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<LinkEntraUserToDsiUseCase>();

        var exception = await Assert.ThrowsExactlyAsync<EntraAccountAlreadyLinkedToDifferentUserException>(async () => {
            await interactor.InvokeAsync(
                new LinkEntraUserToDsiRequest {
                    DsiUserId = UserIdMatchingDsiUserId,
                    EntraUserId = UserEntraOid,
                    FirstName = "Alex",
                    LastName = "Johnson"
                });
        });

        Assert.AreEqual(UserIdMatchingDsiUserId, exception.UserId);
        Assert.AreEqual(UserEntraOid, exception.EntraOid);
        Assert.AreEqual(UserIdMatchingEntraOid, exception.ExistingUserId);
    }

    [TestMethod]
    public async Task Throws_WhenUserAlreadyLinkedToAnEntraAccount()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<LinkEntraUserToDsiUseCase>();

        var exception = await Assert.ThrowsExactlyAsync<UserAlreadyLinkedToEntraAccountException>(async () => {
            await interactor.InvokeAsync(
                new LinkEntraUserToDsiRequest {
                    DsiUserId = UserIdMatchingEntraOid,
                    EntraUserId = EntraOid,
                    FirstName = "Alex",
                    LastName = "Johnson"
                });
        });

        Assert.AreEqual(UserIdMatchingEntraOid, exception.UserId);
        Assert.AreEqual(UserEntraOid, exception.EntraOid);
        Assert.AreEqual(EntraOid, exception.TargetEntraId);
    }

    [TestMethod]
    public async Task LinkEntraUserToDsi_WhenNotPreviouslyLinkedToEntra()
    {
        var (interactor, db, _) = await SetupAsync();

        var response = await interactor.InvokeAsync(
            new LinkEntraUserToDsiRequest {
                DsiUserId = UserIdMatchingDsiUserId,
                EntraUserId = EntraOid,
                FirstName = "Alex",
                LastName = "Johnson"
            }
        );
        var user = db.Users.Single(x => x.Sub == UserIdMatchingDsiUserId);

        Assert.IsNotNull(user);
        Assert.AreEqual(EntraOid, user.EntraOid);
        Assert.IsTrue(user.IsEntra);
        Assert.IsNotNull(user.EntraLinked);
        Assert.AreEqual(new DateTime(2025, 11, 18, 17, 56, 45, DateTimeKind.Utc), user.EntraLinked);
        Assert.AreEqual("Alex", user.FirstName);
        Assert.AreEqual("Johnson", user.LastName);

        Assert.IsInstanceOfType(response, typeof(LinkEntraUserToDsiResponse));
    }

    [TestMethod]
    public async Task WritesToAudit_WhenNotPreviouslyLinkedToEntra()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        var response = await interactor.InvokeAsync(
            new LinkEntraUserToDsiRequest {
                DsiUserId = UserIdMatchingDsiUserId,
                EntraUserId = EntraOid,
                FirstName = "Alex",
                LastName = "Johnson"
            }
        );
        var user = db.Users.Single(x => x.Sub == UserIdMatchingDsiUserId);

        Assert.HasCount(1, capturedAudit);
        var audit = capturedAudit.Single();

        Assert.AreEqual(AuditEventCategoryNames.Auth, audit.EventCategory);
        Assert.AreEqual(AuditAuthEventNames.LinkToExistingUser, audit.EventName);
        Assert.AreEqual($"Linked Entra account with existing DfE Sign-In user {user.Email}.", audit.Message);

        Assert.IsInstanceOfType(response, typeof(LinkEntraUserToDsiResponse));
    }

    [TestMethod]
    public async Task UpdatesNameCorrectly_WhenNotLinked()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        var response = await interactor.InvokeAsync(
            new LinkEntraUserToDsiRequest {
                DsiUserId = UserIdMatchingDsiUserId,
                EntraUserId = EntraOid,
                FirstName = "John",
                LastName = "Doe"
            }
        );

        var user = db.Users.Single(x => x.Sub == UserIdMatchingDsiUserId);

        var audits = capturedAudit.ToList();
        var changeNameAudit = audits.Single(a => a.EventCategory == AuditEventCategoryNames.ChangeName);
        var authAudit = audits.Single(a => a.EventCategory == AuditEventCategoryNames.Auth);

        Assert.AreEqual("John", user.FirstName);
        Assert.AreEqual("Doe", user.LastName);
        Assert.AreEqual(EntraOid, user.EntraOid);
        Assert.IsTrue(user.IsEntra);
        Assert.IsNotNull(user.EntraLinked);

        Assert.HasCount(2, audits);
        Assert.AreEqual($"Successfully changed name to {user.FirstName} {user.LastName}", changeNameAudit.Message);
        Assert.AreEqual(AuditAuthEventNames.LinkToExistingUser, authAudit.EventName);
        Assert.AreEqual($"Linked Entra account with existing DfE Sign-In user {user.Email}.", authAudit.Message);

        Assert.IsInstanceOfType(response, typeof(LinkEntraUserToDsiResponse));

    }

    [TestMethod]
    public async Task UpdatesNameCorrectly_WhenAlreadyLinked()
    {
        var (interactor, db, capturedAudit) = await SetupAsync();

        var response = await interactor.InvokeAsync(
            new LinkEntraUserToDsiRequest {
                DsiUserId = UserIdMatchingEntraOid,
                EntraUserId = UserEntraOid,
                FirstName = "John",
                LastName = "Doe"
            }
        );
        var user = db.Users.Single(x => x.Sub == UserIdMatchingEntraOid);
        var audit = capturedAudit.Single();

        Assert.IsNotNull(user);
        Assert.AreEqual(UserEntraOid, user.EntraOid);
        Assert.IsTrue(user.IsEntra);
        Assert.IsNotNull(user.EntraLinked);
        Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), user.EntraLinked);
        Assert.AreEqual("John", user.FirstName);
        Assert.AreEqual("Doe", user.LastName);

        Assert.AreEqual(AuditEventCategoryNames.ChangeName, audit.EventCategory);
        Assert.AreEqual($"Successfully changed name to {user.FirstName} {user.LastName}", audit.Message);

        Assert.IsInstanceOfType(response, typeof(LinkEntraUserToDsiResponse));
    }
}
