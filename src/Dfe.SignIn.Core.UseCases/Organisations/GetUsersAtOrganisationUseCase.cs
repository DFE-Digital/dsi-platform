using System.Data;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Organisations;

public sealed class GetUsersAtOrganisationUseCase(
    IUnitOfWorkOrganisations uowOrganisations
) : Interactor<GetUsersAtOrganisationRequestNew, GetUsersAtOrganisationResponseNew>
{
    /// <inheritdoc/>
    public override async Task<GetUsersAtOrganisationResponseNew> InvokeAsync(
        InteractionContext<GetUsersAtOrganisationRequestNew> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var query =
                from us in uowOrganisations.Repository<UserServiceEntity>()
                join o in uowOrganisations.Repository<OrganisationEntity>()
                    on us.OrganisationId equals o.Id
                join u in uowOrganisations.Repository<UserEntity>()
                    on us.UserId equals u.Sub
                join s in uowOrganisations.Repository<ServiceEntity>()
                    on us.ServiceId equals s.Id
                join usr in uowOrganisations.Repository<UserServiceRoleEntity>()
                on new {
                    oid = us.OrganisationId,
                    sid = us.ServiceId,
                    uid = us.UserId
                }
                equals new {
                    oid = (Guid?)usr.OrganisationId,
                    sid = (Guid?)usr.ServiceId,
                    uid = usr.UserId
                }
                into usrGroup
                from usr in usrGroup.DefaultIfEmpty()   // LEFT JOIN user_service_roles
                join r in uowOrganisations.Repository<RoleEntity>()
                    on usr.RoleId equals r.Id
                    into roleGroup
                from r in roleGroup.DefaultIfEmpty()    // LEFT JOIN Role
                where s.ClientId == context.Request.ClientId
                    && o.Ukprn == context.Request.ExternalId
                select new UserAtOrganisationNew(
                    u.Sub,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Status,
                    r.Code);

        List<UserAtOrganisationNew> results = await query.ToListAsync();

        //var roles = await uowOrganisations
        //    .Repository<UserServiceEntity>()
        //    .Include(x => x.User)
        //    .Include(x => x.UserServiceRoles)
        //    .Where(us =>
        //        us.Service!.ClientId == "GIAS" &&
        //        us.Organisation!.Ukprn == "10038591")
        //    .Select(us => new UserAtOrganisationNew(
        //        us.User.Sub,
        //        us.User.FirstName,
        //        us.User.LastName,
        //        us.User.Email,
        //        us.User.Status,
        //        "Nobby"))
        //    .ToListAsync(cancellationToken);

        GetUsersAtOrganisationResponseNew responseModel = new() { Users = results };

        return responseModel;
    }
}
