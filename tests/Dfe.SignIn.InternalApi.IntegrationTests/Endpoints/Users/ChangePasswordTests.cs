using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangePassword;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Features.Users.ChangePassword;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class ChangePasswordTests : InternalApiIntegrationEndpointTestBase
{
    private readonly PasswordHasher hasher = new();

    private static string GetEndpoint(Guid userId) => $"internal/users/{userId}/change-password";

    public ChangePasswordTests(InternalApiWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ChangePassword_ReturnsSuccess_UpdatesHashAndSalt_UpgradesToLatestPolicy_WhenCurrentPasswordCorrect()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        const string currentPassword = "CurrentPassw0rd!";
        const string newPassword = "BrandNewPassw0rd!";
        var salt = this.hasher.GenerateSalt();

        var user = EntityFaker.User
            .RuleFor(x => x.Salt, (_, _) => salt)
            .RuleFor(x => x.Password, (_, _) => this.hasher.Hash("v2", currentPassword, salt))
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangePasswordRequest {
            UserId = user.Sub,
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(
            db => db.Users.SingleAsync(x => x.Sub == user.Sub));
        Assert.Equal(this.hasher.HashWithLatestPolicy(newPassword, updatedUser.Salt), updatedUser.Password);
        Assert.False(updatedUser.PasswordResetRequired);

        var policyRow = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserPasswordPolicyEntity?>(
            db => db.UserPasswordPolicies.SingleOrDefaultAsync(x => x.Uid == user.Sub && x.PolicyCode == "v4"));
        Assert.NotNull(policyRow);

        var auditRequest = this.AuditCapturer.CapturedRequests.Single();
        Assert.Equal(AuditEventCategoryNames.ChangePassword, auditRequest.EventCategory);
        Assert.Equal("Successfully changed password", auditRequest.Message);
    }

    [Fact]
    public async Task ChangePassword_Returns400_AndWritesIncorrectPasswordAudit_WhenCurrentPasswordWrong()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();
        var salt = this.hasher.GenerateSalt();

        var user = EntityFaker.User
            .RuleFor(x => x.Salt, (_, _) => salt)
            .RuleFor(x => x.Password, (_, _) => this.hasher.Hash("v2", "TheRealPassword1!", salt))
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangePasswordRequest {
            UserId = user.Sub,
            CurrentPassword = "WrongPassword1!",
            NewPassword = "BrandNewPassw0rd!",
            ConfirmNewPassword = "BrandNewPassw0rd!",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var auditRequest = this.AuditCapturer.CapturedRequests.Single();
        Assert.Equal(AuditChangePasswordEventNames.IncorrectPassword, auditRequest.EventName);
        Assert.True(auditRequest.WasFailure);
    }

    [Fact]
    public async Task ChangePassword_Returns404_WhenUserDoesNotExist()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();
        var userId = Guid.NewGuid();

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(userId), new ChangePasswordRequest {
            UserId = userId,
            CurrentPassword = "whatever",
            NewPassword = "BrandNewPassw0rd!",
            ConfirmNewPassword = "BrandNewPassw0rd!",
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_RotatesHistory_EvictingOldestWhenAtLimit()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        const string currentPassword = "CurrentPassw0rd1!";
        const string newPassword = "BrandNewPassw0rd2!";
        var salt = this.hasher.GenerateSalt();

        var user = EntityFaker.User
            .RuleFor(x => x.Salt, (_, _) => salt)
            .RuleFor(x => x.Password, (_, _) => this.hasher.Hash("v4", currentPassword, salt))
            .Generate();

        this.TimestampInterceptor.Setup(shouldSkipTimestamps: true);

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var policy = new UserPasswordPolicyEntity {
            Id = Guid.NewGuid(),
            Uid = user.Sub,
            PolicyCode = "v4",
            PasswordHistoryLimit = 3,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        await this.InsertEntityAsync<DbDirectoriesContext, UserPasswordPolicyEntity>(policy);

        // Seed 3 history records (oldest to newest)
        var h1 = new PasswordHistoryEntity { Id = Guid.NewGuid(), Password = this.hasher.Hash("v4", "OldPassw0rd1!", "salt1"), Salt = "salt1", CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow.AddDays(-3) };
        var h2 = new PasswordHistoryEntity { Id = Guid.NewGuid(), Password = this.hasher.Hash("v4", "OldPassw0rd2!", "salt2"), Salt = "salt2", CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow.AddDays(-2) };
        var h3 = new PasswordHistoryEntity { Id = Guid.NewGuid(), Password = this.hasher.Hash("v4", "OldPassw0rd3!", "salt3"), Salt = "salt3", CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow.AddDays(-1) };
        await this.InsertEntitiesAsync<DbDirectoriesContext, PasswordHistoryEntity>([h1, h2, h3]);

        var uh1 = new UserPasswordHistoryEntity { PasswordHistoryId = h1.Id, UserSub = user.Sub, CreatedAt = h1.CreatedAt, UpdatedAt = h1.UpdatedAt };
        var uh2 = new UserPasswordHistoryEntity { PasswordHistoryId = h2.Id, UserSub = user.Sub, CreatedAt = h2.CreatedAt, UpdatedAt = h2.UpdatedAt };
        var uh3 = new UserPasswordHistoryEntity { PasswordHistoryId = h3.Id, UserSub = user.Sub, CreatedAt = h3.CreatedAt, UpdatedAt = h3.UpdatedAt };
        await this.InsertEntitiesAsync<DbDirectoriesContext, UserPasswordHistoryEntity>([uh1, uh2, uh3]);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangePasswordRequest {
            UserId = user.Sub,
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userHistory = await this.ExecuteDbContextAsync<DbDirectoriesContext, List<UserPasswordHistoryEntity>>(
            db => db.UserPasswordHistories.Where(x => x.UserSub == user.Sub).ToListAsync());

        Assert.Equal(3, userHistory.Count);
        Assert.DoesNotContain(userHistory, x => x.PasswordHistoryId == h1.Id);

        var addedHistory = await this.ExecuteDbContextAsync<DbDirectoriesContext, PasswordHistoryEntity>(
            db => db.PasswordHistories.SingleAsync(x => x.Password == this.hasher.Hash("v4", currentPassword, salt)));
        Assert.NotNull(addedHistory);
    }

    [Fact]
    public async Task ChangePassword_Returns400_WhenNewPasswordWasUsedRecentlyInHistory()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();
        const string currentPassword = "CurrentPassw0rd1!";
        const string historicalPassword = "HistoricalPassw0rd2!";
        var salt = this.hasher.GenerateSalt();

        var user = EntityFaker.User
            .RuleFor(x => x.Salt, (_, _) => salt)
            .RuleFor(x => x.Password, (_, _) => this.hasher.Hash("v4", currentPassword, salt))
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var histSalt = this.hasher.GenerateSalt();
        var histEntry = new PasswordHistoryEntity {
            Id = Guid.NewGuid(),
            Password = this.hasher.Hash("v2", historicalPassword, histSalt),
            Salt = histSalt,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
        };
        await this.InsertEntityAsync<DbDirectoriesContext, PasswordHistoryEntity>(histEntry);

        var userHist = new UserPasswordHistoryEntity {
            PasswordHistoryId = histEntry.Id,
            UserSub = user.Sub,
            CreatedAt = histEntry.CreatedAt,
            UpdatedAt = histEntry.UpdatedAt,
        };
        await this.InsertEntityAsync<DbDirectoriesContext, UserPasswordHistoryEntity>(userHist);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangePasswordRequest {
            UserId = user.Sub,
            CurrentPassword = currentPassword,
            NewPassword = historicalPassword,
            ConfirmNewPassword = historicalPassword,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ClearsPasswordResetRequiredFlag_WhenSet()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();
        const string currentPassword = "CurrentPassw0rd1!";
        const string newPassword = "BrandNewPassw0rd2!";
        var salt = this.hasher.GenerateSalt();

        var user = EntityFaker.User
            .RuleFor(x => x.Salt, (_, _) => salt)
            .RuleFor(x => x.Password, (_, _) => this.hasher.Hash("v2", currentPassword, salt))
            .RuleFor(x => x.PasswordResetRequired, (_, _) => true)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangePasswordRequest {
            UserId = user.Sub,
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(
            db => db.Users.SingleAsync(x => x.Sub == user.Sub));
        Assert.False(updatedUser.PasswordResetRequired);
    }

    [Fact]
    public async Task ChangePassword_Returns400_WhenNewPasswordMatchesCurrentPassword()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();
        const string currentPassword = "CurrentPassw0rd1!";
        var salt = this.hasher.GenerateSalt();

        var user = EntityFaker.User
            .RuleFor(x => x.Salt, (_, _) => salt)
            .RuleFor(x => x.Password, (_, _) => this.hasher.Hash("v4", currentPassword, salt))
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangePasswordRequest {
            UserId = user.Sub,
            CurrentPassword = currentPassword,
            NewPassword = currentPassword,
            ConfirmNewPassword = currentPassword,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Returns400_WhenConfirmNewPasswordDoesNotMatch()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();
        const string currentPassword = "CurrentPassw0rd1!";
        var salt = this.hasher.GenerateSalt();

        var user = EntityFaker.User
            .RuleFor(x => x.Salt, (_, _) => salt)
            .RuleFor(x => x.Password, (_, _) => this.hasher.Hash("v4", currentPassword, salt))
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangePasswordRequest {
            UserId = user.Sub,
            CurrentPassword = currentPassword,
            NewPassword = "BrandNewPassw0rd1!",
            ConfirmNewPassword = "BrandNewPassw0rd2!",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();
        var response = await anonymousClient.PostAsJsonAsync(GetEndpoint(Guid.NewGuid()), new ChangePasswordRequest {
            UserId = Guid.NewGuid(),
            CurrentPassword = "x",
            NewPassword = "BrandNewPassw0rd!",
            ConfirmNewPassword = "BrandNewPassw0rd!",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("short", "short")] // too short (<8)
    [InlineData("alllowercase1234", "alllowercase1234")] // only 2 categories (lower + digit)
    [InlineData("ALLUPPERCASE1234", "ALLUPPERCASE1234")] // only 2 categories (upper + digit)
    [InlineData("Abcdefghijk", "Abcdefghijk")] // only 2 categories (lower + upper)
    public async Task ChangePassword_Returns400_WhenValidationFails(string newPassword, string confirm)
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();
        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangePasswordRequest {
            UserId = user.Sub,
            CurrentPassword = "irrelevant",
            NewPassword = newPassword,
            ConfirmNewPassword = confirm,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
