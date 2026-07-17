using Bogus;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.TestHelpers.Integration.Data;

public static class EntityFaker
{
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
}
