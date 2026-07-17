using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class GetUserProfileUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        var autoMocker = new AutoMocker();
        var options = new DbContextOptionsBuilder<DbDirectoriesContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dirCtx = new DbDirectoriesContext(options);
        autoMocker.Use(dirCtx);

        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserProfileRequest,
            GetUserProfileUseCase
        >(autoMocker);
    }

    private static async Task<DbDirectoriesContext> SetupFakeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<DbDirectoriesContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new DbDirectoriesContext(options);

        ctx.Users.Add(new UserEntity {
            Sub = Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691"),
            IsEntra = false,
            IsInternalUser = false,
            FirstName = "Alex",
            LastName = "Johnson",
            JobTitle = null,
            Email = "alex.johnson@example.com",
            Password = "",
            Salt = "",
        });

        ctx.Users.Add(new UserEntity {
            Sub = Guid.Parse("74f539fa-5de2-4b15-b983-24ca9612b7cb"),
            IsEntra = true,
            IsInternalUser = true,
            FirstName = "Bob",
            LastName = "Simons",
            JobTitle = "Test Engineer",
            Email = "bob.simons@example.com",
            Password = "",
            Salt = "",
        });

        await ctx.SaveChangesAsync();

        return ctx;
    }

    [TestMethod]
    public async Task Throws_WhenUserNotFound()
    {
        var dirCtx = await SetupFakeDatabaseAsync();
        GetUserProfileUseCase interactor = new(dirCtx);

        Guid nonExistentUserId = Guid.Parse("6d690a96-c392-4482-b750-733ea472bc96");

        var exception = await Assert.ThrowsExactlyAsync<UserNotFoundException>(()
            => interactor.InvokeAsync(
                new GetUserProfileRequest {
                    UserId = nonExistentUserId,
                }
            ));
        Assert.AreEqual(nonExistentUserId, exception.UserId);
    }

    public static IEnumerable<object[]> GetCasesForReturnsExpectedProfile()
    {
        yield return new object[] {
            Guid.Parse("3ed6826f-6854-4adf-b65f-5b2ace7c8691"),
            new GetUserProfileResponse {
                IsEntra = false,
                IsInternalUser = false,
                FirstName = "Alex",
                LastName = "Johnson",
                JobTitle = null,
                EmailAddress = "alex.johnson@example.com",
            }
        };
        yield return new object[] {
            Guid.Parse("74f539fa-5de2-4b15-b983-24ca9612b7cb"),
            new GetUserProfileResponse {
                IsEntra = true,
                IsInternalUser = true,
                FirstName = "Bob",
                LastName = "Simons",
                JobTitle = "Test Engineer",
                EmailAddress = "bob.simons@example.com",
            }
        };
    }

    [TestMethod]
    [DynamicData(nameof(GetCasesForReturnsExpectedProfile), DynamicDataSourceType.Method)]
    public async Task ReturnsExpectedProfile(Guid userId, GetUserProfileResponse expectedResponse)
    {
        var dirCtx = await SetupFakeDatabaseAsync();
        GetUserProfileUseCase interactor = new(dirCtx);

        var response = await interactor.InvokeAsync(
            new GetUserProfileRequest {
                UserId = userId,
            }
        );

        Assert.AreEqual(expectedResponse, response);
    }
}
