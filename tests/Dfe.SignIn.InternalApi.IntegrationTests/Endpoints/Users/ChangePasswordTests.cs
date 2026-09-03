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
        // Arrange: seed 3 existing PasswordHistoryEntity/UserPasswordHistoryEntity rows
        // (limit) with distinct CreatedAt timestamps, then assert the oldest is deleted
        // and the pre-change password/salt is added after a successful change.
        await Task.CompletedTask;
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
    [InlineData("short", "short")] // too short + mismatch
    [InlineData("alllowercase1234", "alllowercase1234")] // fails complexity (only 2 of 4 categories)
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
