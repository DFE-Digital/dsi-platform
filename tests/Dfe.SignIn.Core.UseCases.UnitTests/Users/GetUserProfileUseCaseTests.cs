using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.UseCases.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class GetUserProfileUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserProfileRequest,
            GetUserProfileUseCase
        >();
    }

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryDirectoriesDb();

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
    }

    [TestMethod]
    public async Task Throws_WhenUserNotFound()
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetUserProfileUseCase>();

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
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);
        var interactor = autoMocker.CreateInstance<GetUserProfileUseCase>();

        var response = await interactor.InvokeAsync(
            new GetUserProfileRequest {
                UserId = userId,
            }
        );

        Assert.AreEqual(expectedResponse, response);
    }
}
