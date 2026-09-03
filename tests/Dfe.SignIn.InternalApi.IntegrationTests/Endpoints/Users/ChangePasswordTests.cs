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

        var policy = EntityFaker.UserPasswordPolicy
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserPasswordPolicyEntity>(policy);

        // Seed 3 history records (oldest to newest)
        var historyEntities = EntityFaker.PasswordHistory
            .RuleFor(x => x.Id, (_, _) => Guid.NewGuid())
            .RuleFor(x => x.CreatedAt, f => DateTime.UtcNow.AddDays(-(3 - f.IndexFaker)))
            .RuleFor(x => x.UpdatedAt, f => DateTime.UtcNow.AddDays(-(3 - f.IndexFaker)))
            .Generate(3);

        var h1 = historyEntities[0];
        var h2 = historyEntities[1];
        var h3 = historyEntities[2];
        await this.InsertEntitiesAsync<DbDirectoriesContext, PasswordHistoryEntity>([h1, h2, h3]);

        var userHistories = EntityFaker.UserPasswordHistory
            .RuleFor(x => x.UserSub, user.Sub)
            .RuleFor(x => x.PasswordHistoryId, f => historyEntities[f.IndexFaker].Id)
            .RuleFor(x => x.CreatedAt, f => historyEntities[f.IndexFaker].CreatedAt)
            .RuleFor(x => x.UpdatedAt, f => historyEntities[f.IndexFaker].UpdatedAt)
            .Generate(historyEntities.Count());

        var uh1 = userHistories[0];
        var uh2 = userHistories[1];
        var uh3 = userHistories[2];
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

        var historyEntity = EntityFaker.PasswordHistory
            .RuleFor(x => x.Password, (_, _) => this.hasher.Hash("v2", historicalPassword, histSalt))
            .RuleFor(x => x.Salt, (_, _) => histSalt)
            .RuleFor(x => x.CreatedAt, f => DateTime.UtcNow.AddDays(-1))
            .RuleFor(x => x.UpdatedAt, f => DateTime.UtcNow.AddDays(-1))
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, PasswordHistoryEntity>(historyEntity);

        var userHistory = EntityFaker.UserPasswordHistory
            .RuleFor(x => x.UserSub, (_, _) => user.Sub)
            .RuleFor(x => x.PasswordHistoryId, (_, _) => historyEntity.Id)
            .RuleFor(x => x.CreatedAt, f => historyEntity.CreatedAt)
            .RuleFor(x => x.UpdatedAt, f => historyEntity.UpdatedAt)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserPasswordHistoryEntity>(userHistory);

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
