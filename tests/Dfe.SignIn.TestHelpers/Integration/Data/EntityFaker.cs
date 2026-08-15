using Bogus;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.TestHelpers.Integration.Data;

/// <summary>
/// Provides methods to generate fake entities for testing purposes.
/// </summary>
public static class EntityFaker
{
    private const short ActiveUserOrganisationStatus = 1;

    // login.dfe.organisations\src\infrastructure\repository\index.js

    public static Faker<UserEntity> User => new Faker<UserEntity>()
        .RuleFor( x => x.Sub, f => f.Random.Guid() )
        .RuleFor( x => x.Email, f => f.Internet.Email() )
        .RuleFor( x => x.FirstName, f => f.Name.FirstName() )
        .RuleFor( x => x.LastName, f => f.Name.LastName() )
        .RuleFor( x => x.Password, f => f.Internet.Password() )
        .RuleFor( x => x.Salt, f => f.Random.AlphaNumeric( 32 ) )
        .RuleFor( x => x.Status, _ => (short)AccountStatus.Active )
        .RuleFor( x => x.CreatedAt, f => f.Date.Past( 2 ) )
        .RuleFor( x => x.UpdatedAt, ( f, user ) => f.Date.Between( user.CreatedAt, DateTime.UtcNow ) )
        .RuleFor( x => x.JobTitle, ( _, _ ) => "Old Title" );

    public static Faker<OrganisationEntity> Organisation => new Faker<OrganisationEntity>()
        .RuleFor( x => x.Id, f => f.Random.Guid() )
        .RuleFor( x => x.Name, f => f.Company.CompanyName() )
        .RuleFor( x => x.Category, _ => "001" )
        .RuleFor( x => x.Status, _ => (int)OrganisationStatus.Open )
        .RuleFor( x => x.CreatedAt, f => f.Date.Past( 2 ) )
        .RuleFor( x => x.UpdatedAt, ( f, org ) => f.Date.Between( org.CreatedAt, DateTime.UtcNow ) );

    public static Faker<UserOrganisationEntity> UserOrganisation => new Faker<UserOrganisationEntity>()
        .RuleFor( x => x.UserId, f => f.Random.Guid() )
        .RuleFor( x => x.OrganisationId, f => f.Random.Guid() )
        .RuleFor( x => x.RoleId, _ => OrganisationRoles.EndUser.Id )
        .RuleFor( x => x.Status, _ => ActiveUserOrganisationStatus )
        .RuleFor( x => x.CreatedAt, f => f.Date.Past( 2 ) )
        .RuleFor( x => x.UpdatedAt, ( f, userOrg ) => f.Date.Between( userOrg.CreatedAt, DateTime.UtcNow ) );

    public static Faker<UserOrganisationRequestEntity> UserOrganisationRequest => new Faker<UserOrganisationRequestEntity>()
        .RuleFor( x => x.Id, f => f.Random.Guid() )
        .RuleFor( x => x.UserId, f => f.Random.Guid() )
        .RuleFor( x => x.OrganisationId, f => f.Random.Guid() )
        .RuleFor( x => x.Status, _ => 0 ) // Assuming 0 is the default status for a pending request
        .RuleFor( x => x.ActionedAt, _ => null )
        .RuleFor( x => x.CreatedAt, f => f.Date.Past( 2 ) )
        .RuleFor( x => x.UpdatedAt, ( f, request ) => f.Date.Between( request.CreatedAt, DateTime.UtcNow ) );

    //  model.organisationRequestStatus = [
    //  { id: -1, name: "Rejected" },
    //  { id: 0, name: "Pending" },
    //  { id: 1, name: "Approved" },
    //  { id: 2, name: "Overdue" },
    //  { id: 3, name: "No Approvers" },
    //];

    //  model.organisationStatus = [
    //  { id: 0, name: "Hidden", tagColor: "grey" },
    //  { id: 1, name: "Open", tagColor: "green" },
    //  { id: 2, name: "Closed", tagColor: "red" },
    //  { id: 3, name: "Proposed to close", tagColor: "orange" },
    //  { id: 4, name: "Proposed to open", tagColor: "blue" },
    //  { id: 5, name: "Dissolved", tagColor: "red" },
    //  { id: 6, name: "In Liquidation", tagColor: "red" },
    //  { id: 8, name: "Locked Duplicate", tagColor: "purple" },
    //  { id: 9, name: "Created in error", tagColor: "red" },
    //  { id: 10, name: "Locked Restructure", tagColor: "purple" },
    //];

    //    await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>( new UserOrganisationEntity {
    //        UserId = userId,
    //            OrganisationId = org.Id,
    //            RoleId = OrganisationRoles.EndUser.Id,
    //            CreatedAt = DateTime.UtcNow,
    //            UpdatedAt = DateTime.UtcNow,
    //            Status = ActiveUserOrganisationStatus
    //    } );

    //        // Seed pending request for the organisation
    //        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationRequestEntity>( new UserOrganisationRequestEntity {
    //        Id = Guid.NewGuid(),
    //            UserId = Guid.NewGuid(),
    //            OrganisationId = org.Id,
    //            Status = 0,
    //            ActionedAt = null,
    //            CreatedAt = DateTime.UtcNow,
    //            UpdatedAt = DateTime.UtcNow
    //} );

    public static Faker<UserCodeEntity> UserCode => new Faker<UserCodeEntity>()
                .RuleFor( x => x.Uid, f => f.Random.Guid() )
                .RuleFor( x => x.CodeType, _ => "changeemail" )
                .RuleFor( x => x.Code, f => f.Random.AlphaNumeric( 7 ) )
                .RuleFor( x => x.Email, f => f.Internet.Email() )
                .RuleFor( x => x.ClientId, _ => "test-client" )
                .RuleFor( x => x.RedirectUri, _ => "n/a" )
                .RuleFor( x => x.ContextData, _ => null )
                .RuleFor( x => x.CreatedAt, f => f.Date.Past( 2 ) )
                .RuleFor( x => x.UpdatedAt, ( f, code ) => f.Date.Between( code.CreatedAt, DateTime.UtcNow ) );
}
