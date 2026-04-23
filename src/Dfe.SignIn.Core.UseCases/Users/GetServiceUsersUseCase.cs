using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for retrieving all service users with pagination and optional filters.
/// </summary>
public sealed class GetServiceUsersUseCase(
    IUnitOfWorkOrganisations uowOrganisations,
    IUnitOfWorkDirectories uowDirectories
) : Interactor<GetServiceUsersRequest, GetServiceUsersResponse>
{
    /// <inheritdoc/>
    public override async Task<GetServiceUsersResponse> InvokeAsync(
        InteractionContext<GetServiceUsersRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var userServicesQuery = uowOrganisations.Repository<UserServiceEntity>()
            .Include(x => x.Organisation)
            .Where(x => x.ServiceId == context.Request.ApplicationId);

        //var totalRecords = await userServicesQuery.CountAsync(cancellationToken);
        //var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var userServices = await userServicesQuery
            .OrderBy(us => us.CreatedAt)
            //.Skip((request.PageNumber - 1) * request.PageSize)
            //.Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetServiceUsersResponse();

        //var request = context.Request;

        //// 1. Resolve ServiceId from ClientId
        //var service = await uowOrganisations.Repository<ServiceEntity>()
        //    .FirstOrDefaultAsync(s => s.ClientId == request.ClientId, cancellationToken);

        //if (service == null) {
        //    return new GetServiceUsersResponse {
        //        Users = [],
        //        NumberOfRecords = 0,
        //        Page = request.PageNumber,
        //        NumberOfPages = 0
        //    };
        //}

        //// 2. Query UserServices (Organisations DB)
        //var userServicesQuery = uowOrganisations.Repository<UserServiceEntity>()
        //    .Include(us => us.Organisation)
        //    .Where(us => us.ServiceId == service.Id);

        //if (!string.IsNullOrEmpty(request.Status)) {
        //    if (short.TryParse(request.Status, out var statusValue)) {
        //        userServicesQuery = userServicesQuery.Where(us => us.Status == statusValue);
        //    }
        //}

        //if (request.DateFrom.HasValue) {
        //    userServicesQuery = userServicesQuery.Where(us => us.CreatedAt >= request.DateFrom.Value);
        //}
        //if (request.DateTo.HasValue) {
        //    userServicesQuery = userServicesQuery.Where(us => us.CreatedAt <= request.DateTo.Value);
        //}

        //var totalRecords = await userServicesQuery.CountAsync(cancellationToken);
        //var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        //var userServices = await userServicesQuery
        //    .OrderBy(us => us.CreatedAt)
        //    .Skip((request.PageNumber - 1) * request.PageSize)
        //    .Take(request.PageSize)
        //    .ToListAsync(cancellationToken);

        //if (userServices.Count == 0) {
        //    return new GetServiceUsersResponse {
        //        Users = [],
        //        NumberOfRecords = totalRecords,
        //        Page = !string.IsNullOrEmpty(request.Status) || request.DateFrom.HasValue || request.DateTo.HasValue ? 0 : request.PageNumber,
        //        NumberOfPages = totalPages
        //    };
        //}

        //var userIds = userServices.Select(us => us.UserId).Distinct().ToList();

        //// 3. Query User Profiles (Directories DB)
        //var userProfiles = await uowDirectories.Repository<UserEntity>()
        //    .Where(u => userIds.Contains(u.Sub))
        //    .ToListAsync(cancellationToken);

        //// 4. Query Service Roles (Organisations DB)
        //var userServiceRoles = await uowOrganisations.Repository<UserServiceRoleEntity>()
        //    .Include(usr => usr.Role)
        //    .Where(usr => usr.ServiceId == service.Id && userIds.Contains(usr.UserId))
        //    .ToListAsync(cancellationToken);

        //// 5. Map and Join
        //var mappedUsers = userServices.Select(userService => {
        //    var profile = userProfiles.FirstOrDefault(u => u.Sub == userService.UserId);

        //    var rolesForUserOrg = userServiceRoles
        //        .Where(usr => usr.UserId == userService.UserId && usr.OrganisationId == userService.OrganisationId)
        //        .Select(usr => new ServiceUserRoleDto {
        //            Id = usr.Role.Id.ToString(),
        //            Name = usr.Role.Name,
        //            Code = usr.Role.Code,
        //            NumericId = usr.Role.NumericId.ToString(),
        //            Status = usr.Role.Status
        //        })
        //        .ToList();

        //    var dto = new ServiceUserDto {
        //        UserId = userService.UserId,
        //        ApprovedAt = ToUtc(userService.CreatedAt),
        //        UpdatedAt = ToUtc(userService.UpdatedAt),
        //        Organisation = userService.Organisation != null ? new ServiceUserOrganisationDto {
        //            Id = userService.Organisation.Id,
        //            Name = userService.Organisation.Name,
        //            Category = userService.Organisation.Category,
        //            Type = userService.Organisation.Type,
        //            Urn = userService.Organisation.Urn,
        //            Uid = userService.Organisation.Uid,
        //            Ukprn = userService.Organisation.Ukprn,
        //            EstablishmentNumber = userService.Organisation.EstablishmentNumber,
        //            Status = userService.Organisation.Status,
        //            ClosedOn = userService.Organisation.ClosedOn,
        //            Address = userService.Organisation.Address,
        //            Telephone = userService.Organisation.Telephone,
        //            StatutoryLowAge = userService.Organisation.StatutoryLowAge,
        //            StatutoryHighAge = userService.Organisation.StatutoryHighAge,
        //            LegacyId = userService.Organisation.LegacyId,
        //            CompanyRegistrationNumber = userService.Organisation.CompanyRegistrationNumber,
        //            PhaseOfEducation = userService.Organisation.PhaseOfEducation,
        //            ProviderProfileId = userService.Organisation.ProviderProfileId,
        //            SourceSystem = userService.Organisation.SourceSystem
        //        } : null,
        //        Roles = rolesForUserOrg,
        //        RoleName = rolesForUserOrg.FirstOrDefault()?.Name,
        //        RoleId = rolesForUserOrg.FirstOrDefault()?.Id,
        //        Email = profile?.Email ?? string.Empty,
        //        FamilyName = profile?.LastName,
        //        GivenName = profile?.FirstName,
        //        UserStatus = profile?.Status.ToString()
        //    };

        //    return dto;
        //}).ToList();

        //if (userIds.Count > 0 && mappedUsers.All(u => string.IsNullOrEmpty(u.Email))) {
        //    return new GetServiceUsersResponse {
        //        Users = [],
        //        NumberOfRecords = 0,
        //        Page = 0,
        //        NumberOfPages = 0
        //    };
        //}

        //return new GetServiceUsersResponse {
        //    Users = mappedUsers,
        //    NumberOfRecords = totalRecords,
        //    Page = request.PageNumber,
        //    NumberOfPages = totalPages
        //};
    }

    //private static DateTime ToUtc(DateTime date)
    //{
    //    return date.Kind == DateTimeKind.Unspecified
    //        ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
    //        : date.ToUniversalTime();
    //}

    //private static DateTime? ToUtc(DateTime? date)
    //{
    //    if (!date.HasValue)
    //        return null;
    //    return ToUtc(date.Value);
    //}
}
