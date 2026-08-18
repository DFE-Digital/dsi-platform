using Bogus;
using Dfe.SignIn.Core.Contracts.Features.Shared;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
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
    public static Faker<UserEntity> User => new Faker<UserEntity>()
        .RuleFor(x => x.Sub, f => f.Random.Guid())
        .RuleFor(x => x.Email, f => f.Internet.Email())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.Password, f => f.Internet.Password())
        .RuleFor(x => x.Salt, f => f.Random.AlphaNumeric(32))
        .RuleFor(x => x.Status, _ => (short)AccountStatus.Active)
        .RuleFor(x => x.CreatedAt, f => f.Date.Past(2))
        .RuleFor(x => x.UpdatedAt, (f, user) => f.Date.Between(user.CreatedAt, DateTime.UtcNow))
        .RuleFor(x => x.JobTitle, (_, _) => "Old Title");

    public static Faker<OrganisationEntity> Organisation => new Faker<OrganisationEntity>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Name, f => f.Company.CompanyName())
        .RuleFor(x => x.Category, _ => "001")
        .RuleFor(x => x.Status, _ => (int)OrganisationStatus.Open)
        .RuleFor(x => x.CreatedAt, f => f.Date.Past(2))
        .RuleFor(x => x.UpdatedAt, (f, org) => f.Date.Between(org.CreatedAt, DateTime.UtcNow));

    public static Faker<UserOrganisationEntity> UserOrganisation => new Faker<UserOrganisationEntity>()
        .RuleFor(x => x.UserId, f => f.Random.Guid())
        .RuleFor(x => x.OrganisationId, f => f.Random.Guid())
        .RuleFor(x => x.RoleId, _ => OrganisationRole.EndUser.Value)
        .RuleFor(x => x.Status, _ => UserOrganisationStatus.Approved.Value)
        .RuleFor(x => x.CreatedAt, f => f.Date.Past(2))
        .RuleFor(x => x.UpdatedAt, (f, userOrg) => f.Date.Between(userOrg.CreatedAt, DateTime.UtcNow));

    public static Faker<UserOrganisationRequestEntity> UserOrganisationRequest => new Faker<UserOrganisationRequestEntity>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.UserId, f => f.Random.Guid())
        .RuleFor(x => x.OrganisationId, f => f.Random.Guid())
        .RuleFor(x => x.Status, _ => OrganisationRequestStatus.Pending.Value)
        .RuleFor(x => x.ActionedAt, _ => null)
        .RuleFor(x => x.CreatedAt, f => f.Date.Past(2))
        .RuleFor(x => x.UpdatedAt, (f, request) => f.Date.Between(request.CreatedAt, DateTime.UtcNow));

    public static Faker<UserServiceRequestEntity> UserServiceRequest => new Faker<UserServiceRequestEntity>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.UserId, f => f.Random.Guid())
        .RuleFor(x => x.ServiceId, f => f.Random.Guid())
        .RuleFor(x => x.OrganisationId, f => f.Random.Guid())
        .RuleFor(x => x.Status, _ => ServiceRequestStatus.Pending.Value)
        .RuleFor(x => x.RequestType, _ => ServiceRequestType.Service.Value)
        .RuleFor(x => x.ActionedAt, _ => null)
        .RuleFor(x => x.CreatedAt, f => f.Date.Past(2))
        .RuleFor(x => x.UpdatedAt, (f, userService) => f.Date.Between(userService.CreatedAt, DateTime.UtcNow));

    public static Faker<UserCodeEntity> UserCode => new Faker<UserCodeEntity>()
        .RuleFor(x => x.Uid, f => f.Random.Guid())
        .RuleFor(x => x.CodeType, _ => UserCodeType.ChangeEmail.Value)
        .RuleFor(x => x.Code, f => f.Random.AlphaNumeric(7))
        .RuleFor(x => x.Email, f => f.Internet.Email())
        .RuleFor(x => x.ClientId, _ => "test-client")
        .RuleFor(x => x.RedirectUri, _ => "n/a")
        .RuleFor(x => x.ContextData, _ => null)
        .RuleFor(x => x.CreatedAt, f => f.Date.Past(2))
        .RuleFor(x => x.UpdatedAt, (f, code) => f.Date.Between(code.CreatedAt, DateTime.UtcNow));
}
