using System.Net;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmailAddress;

[Trait( "Category", "Integration" )]
public sealed class CancelChangeChangeEmailTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "/internal/users/{userId}/cancel-change-email";

    private static string GetEndpointForUser( Guid userId ) => endpoint.Replace( "{userId}", userId.ToString() );

    public CancelChangeChangeEmailTests( InternalApiWebApplicationFactory factory )
        : base( factory )
    {
    }

    [Fact]
    public async Task CancelChangeEmail_ReturnsSuccess_WritesAudit_AndDeletesCode()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var user = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "john.doe@old.example.com" )
            .Generate();

        var pendingCode = EntityFaker.UserCode
            .RuleFor( x => x.Uid, ( _, _ ) => user.Sub )
            .RuleFor( x => x.CodeType, ( _, _ ) => "changeemail" )
            .RuleFor( x => x.Code, ( _, _ ) => "ABC1234" )
            .RuleFor( x => x.Email, ( _, _ ) => "john.doe@new.example.com" )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>( pendingCode );

        var path = GetEndpointForUser( user.Sub );
        var response = await authenticatedClient.DeleteAsync( path );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var dbCode = await GetPendingChangeEmailCode( assertionDbContext, user.Sub );
        Assert.Null( dbCode );

        Assert.NotNull( auditMock.CapturedRequest );
        Assert.Equal( AuditEventCategoryNames.ChangeEmail, auditMock.CapturedRequest.EventCategory );
        Assert.Equal( AuditChangeEmailEventNames.CancelChangeEmail, auditMock.CapturedRequest.EventName );
        Assert.Contains( user.Sub.ToString(), auditMock.CapturedRequest.Message );
    }

    [Fact]
    public async Task CancelChangeEmail_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.SetupClient().Build().Client;
        var userId = Guid.NewGuid();

        var response = await anonymousClient.PostAsync( GetEndpointForUser( userId ), new StringContent( string.Empty ) );

        Assert.Equal( HttpStatusCode.Unauthorized, response.StatusCode );
    }

    [Fact]
    public async Task CancelChangeEmail_DeletesOnlyTargetPendingCode_AndLeavesUserEmailUnchanged()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var targetUser = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "target.old@example.com" )
            .Generate();

        var otherUser = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "other@example.com" )
            .Generate();

        var targetCode = EntityFaker.UserCode
            .RuleFor( x => x.Uid, ( _, _ ) => targetUser.Sub )
            .RuleFor( x => x.CodeType, ( _, _ ) => "changeemail" )
            .RuleFor( x => x.Code, ( _, _ ) => "TARGET123" )
            .RuleFor( x => x.Email, ( _, _ ) => "target.new@example.com" )
            .Generate();

        var otherCode = EntityFaker.UserCode
            .RuleFor( x => x.Uid, ( _, _ ) => otherUser.Sub )
            .RuleFor( x => x.CodeType, ( _, _ ) => "changeemail" )
            .RuleFor( x => x.Code, ( _, _ ) => "OTHER123" )
            .RuleFor( x => x.Email, ( _, _ ) => "other.new@example.com" )
            .Generate();

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>( [targetUser, otherUser] );
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>( targetCode );
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>( otherCode );

        var response = await authenticatedClient.DeleteAsync( GetEndpointForUser( targetUser.Sub ) );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var targetUserRow = await assertionDbContext.Users.SingleAsync( x => x.Sub == targetUser.Sub );
        Assert.Equal( "target.old@example.com", targetUserRow.Email );

        var targetPending = await GetPendingChangeEmailCode( assertionDbContext, targetUser.Sub );
        Assert.Null( targetPending );

        var otherPending = await GetPendingChangeEmailCode( assertionDbContext, otherUser.Sub );
        Assert.NotNull( otherPending );
        Assert.Equal( "other.new@example.com", otherPending.Email );
    }

    [Fact]
    public async Task CancelChangeEmail_DoesNotWriteSuccessPathAudit_WhenDeleteFails()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var user = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "john.doe@old.example.com" )
            .Generate();

        var pendingCode = EntityFaker.UserCode
            .RuleFor( x => x.Uid, ( _, _ ) => user.Sub )
            .RuleFor( x => x.CodeType, ( _, _ ) => "changeemail" )
            .RuleFor( x => x.Code, ( _, _ ) => "ABC1234" )
            .RuleFor( x => x.Email, ( _, _ ) => "john.doe@new.example.com" )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>( pendingCode );

        this.TestTimestampInterceptor.ShouldFail = true;

        var response = await authenticatedClient.PostAsync( GetEndpointForUser( user.Sub ), new StringContent( string.Empty ) );

        Assert.NotEqual( HttpStatusCode.OK, response.StatusCode );
        Assert.DoesNotContain( auditMock.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.CancelChangeEmail && !x.WasFailure );
    }

    [Fact]
    public async Task CancelChangeEmail_ReturnsSuccess_WhenNoPendingCodeExists()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;

        var user = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "john.doe@old.example.com" )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var response = await authenticatedClient.DeleteAsync( GetEndpointForUser( user.Sub ) );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var dbCode = await GetPendingChangeEmailCode( assertionDbContext, user.Sub );
        Assert.Null( dbCode );
    }

    private static async Task<UserCodeEntity?> GetPendingChangeEmailCode( DbDirectoriesContext dbContext, Guid userId )
        => await dbContext.UserCodes.SingleOrDefaultAsync( x => x.Uid == userId && x.CodeType == "changeemail" );
}
