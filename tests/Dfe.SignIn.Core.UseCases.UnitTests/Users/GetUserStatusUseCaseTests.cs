
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class GetUserStatusUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserStatusRequest,
            GetUserStatusUseCase
        >();
    }

    private static readonly Guid UserIdMatchingEmail = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691");
    private static readonly Guid UserIdMatchingEntraOid = Guid.Parse("74f539fa-5de2-4b15-b983-24ca9612b7cb");
    private static readonly Guid UserEntraOid = Guid.Parse("ecf31b2d-03e8-4f00-9035-32d2dd4a9ed3");

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker)
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
    }

    [TestMethod]
    public async Task UserDoesNotExistByEntraOid_ReturnsExpectedObject()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetUserStatusUseCase>();

        var result = await interactor.InvokeAsync(
            new GetUserStatusRequest {
                EntraUserId = Guid.Parse("6d690a96-c392-4482-b750-733ea472bc96")
            });

        var expectedResult = new GetUserStatusResponse {
            UserExists = false,
            AccountStatus = null,
            UserId = null
        };

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    public async Task UserDoesNotExistByEmail_ReturnsExpectedObject()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetUserStatusUseCase>();

        var result = await interactor.InvokeAsync(
            new GetUserStatusRequest {
                EmailAddress = "bob@bob.com"
            });

        var expectedResult = new GetUserStatusResponse {
            UserExists = false,
            AccountStatus = null,
            UserId = null
        };

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    public async Task UserExistByEmail_ReturnsExpectedObject()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetUserStatusUseCase>();

        var result = await interactor.InvokeAsync(
            new GetUserStatusRequest {
                EmailAddress = "alex.johnson@example.com"
            });

        var expectedResult = new GetUserStatusResponse {
            UserExists = true,
            AccountStatus = AccountStatus.Active,
            UserId = UserIdMatchingEmail
        };

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    public async Task UserExistByEntraOid_ReturnsExpectedObject()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetUserStatusUseCase>();

        var result = await interactor.InvokeAsync(
            new GetUserStatusRequest {
                EntraUserId = UserEntraOid
            });

        var expectedResult = new GetUserStatusResponse {
            UserExists = true,
            AccountStatus = AccountStatus.Inactive,
            UserId = UserIdMatchingEntraOid
        };

        Assert.AreEqual(expectedResult, result);
    }
}
