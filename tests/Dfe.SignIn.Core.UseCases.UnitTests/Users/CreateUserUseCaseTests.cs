using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class CreateUserUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            CreateUserRequest,
            CreateUserUseCase
        >();
    }

    private static readonly Guid UserIdMatchingEmail = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691");
    private static readonly Guid UserIdMatchingEntraOid = Guid.Parse("74f539fa-5de2-4b15-b983-24ca9612b7cb");
    private static readonly Guid UserEntraOid = Guid.Parse("ecf31b2d-03e8-4f00-9035-32d2dd4a9ed3");

    private static async Task<DbDirectoriesContext> SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryDirectoriesDb();

        ctx.Users.Add(new UserEntity {
            Sub = UserIdMatchingEmail,
            IsEntra = false,
            FirstName = "Alex",
            LastName = "Johnson",
            Email = "alex.johnson@example.com",
            Password = "",
            Salt = "",
            Status = 1
        });

        ctx.Users.Add(new UserEntity {
            Sub = UserIdMatchingEntraOid,
            IsEntra = true,
            FirstName = "Bob",
            LastName = "Simons",
            Email = "bob.simons@example.com",
            Password = "",
            Salt = "",
            EntraOid = UserEntraOid,
            Status = 0
        });

        await ctx.SaveChangesAsync();

        return ctx;
    }

    [TestMethod]
    public async Task ThrowsWhen_EmailAlreadyExists()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<CreateUserUseCase>();

        await Assert.ThrowsExactlyAsync<CannotCreateNewUserException>(()
            => interactor.InvokeAsync(
                new CreateUserRequest {
                    EmailAddress = "alex.johnson@example.com",
                    FirstName = "",
                    LastName = "",
                    EntraUserId = Guid.Parse("4003c05c-8550-4f0d-8d96-43cec66a3e6a")
                }
            ));
    }

    [TestMethod]
    public async Task ThrowsWhen_EntraUserIdAlreadyExists()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<CreateUserUseCase>();

        await Assert.ThrowsExactlyAsync<CannotCreateNewUserException>(()
            => interactor.InvokeAsync(
                new CreateUserRequest {
                    EmailAddress = "joe.brown@example.com",
                    FirstName = "",
                    LastName = "",
                    EntraUserId = UserEntraOid
                }
            ));
    }

    [TestMethod]
    public async Task ShouldCreateNewUser()
    {
        var autoMocker = new AutoMocker();
        var db = await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<CreateUserUseCase>();

        var user = await interactor.InvokeAsync(
            new CreateUserRequest {
                EmailAddress = "joe.brown@example.com",
                FirstName = "joe",
                LastName = "brown",
                EntraUserId = Guid.Parse("fa70e11c-f1eb-4bab-9fa0-ff36a9620066")
            });

        Assert.IsNotNull(user);

        var matchingUser = db.Users.FirstOrDefault(x => x.Sub == user.UserId);

        Assert.IsNotNull(user);
        Assert.AreEqual("joe.brown@example.com", matchingUser?.Email);
        Assert.AreEqual("joe", matchingUser?.FirstName);
        Assert.AreEqual("brown", matchingUser?.LastName);
        Assert.AreEqual((short)1, matchingUser?.Status);
        Assert.IsTrue(matchingUser?.IsEntra);
        Assert.IsNotNull(matchingUser?.EntraLinked);
        Assert.IsNotNull(matchingUser?.CreatedAt);
        Assert.IsNotNull(matchingUser?.UpdatedAt);
        Assert.AreEqual(Guid.Parse("fa70e11c-f1eb-4bab-9fa0-ff36a9620066"), matchingUser.EntraOid);
    }
}
