using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for retrieving all service users with pagination and optional filters.
/// </summary>
public sealed class GetServiceUsersUseCase(
    IUnitOfWorkOrganisations uowOrganisations
) : Interactor<GetServiceUsersRequest, GetServiceUsersResponse>
{
    /// <inheritdoc/>
    public override async Task<GetServiceUsersResponse> InvokeAsync(
        InteractionContext<GetServiceUsersRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var applicationId = context.Request.ApplicationId;
        var pageSize = context.Request.PageSize;
        var pageNumber = context.Request.PageNumber;

        // 2. Query UserServices (Organisations DB)
        var userServicesQuery =
            from us in uowOrganisations.Repository<UserServiceEntity>()
            join u in uowOrganisations.Repository<UserEntity>() on us.UserId equals u.Sub
            join org in uowOrganisations.Repository<OrganisationEntity>() on us.OrganisationId equals org.Id
            where us.ServiceId == applicationId
            select new { UserService = us, Organisation = org, User = u };

        var totalRecords = await userServicesQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var userServices = await userServicesQuery
            .OrderBy(us => us.UserService.UserId)
            .ThenBy(us => us.UserService.OrganisationId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = userServices.Select(us => us.UserService.UserId).Distinct().ToList();

        // 4. Query Service Roles (Organisations DB)
        var userServiceRoles = await uowOrganisations.Repository<UserServiceRoleEntity>()
            .Include(usr => usr.Role)
            .Where(usr => usr.ServiceId == applicationId && userIds.Contains(usr.UserId))
            .ToListAsync(cancellationToken);

        var userOrganisationRoles = await uowOrganisations.Repository<UserOrganisationEntity>()
            .Where(uor => userIds.Contains(uor.UserId))
            .Select(x => new {
                x.UserId,
                x.OrganisationId,
                x.RoleId
            })
            .ToListAsync(cancellationToken);

        // 5. Map and Join
        var mappedUsers = userServices.Select(userService => {
            var userServiceRolesForOrganisation = userServiceRoles
                .Where(usr => usr.UserId == userService.UserService.UserId && usr.OrganisationId == userService.UserService.OrganisationId)
                .Select(usr => new ServiceUserRoleDto {
                    Id = usr.Role?.Id.ToString() ?? string.Empty,
                    Name = usr.Role?.Name ?? string.Empty,
                    Code = usr.Role?.Code ?? string.Empty,
                    NumericId = usr.Role?.NumericId.ToString() ?? string.Empty,
                    Status = usr.Role?.Status ?? (int)ApplicationRoleStatus.Inactive //todo: is this the correct default?
                })
                .ToList();

            var userOrganisationRole = userOrganisationRoles
                .Where(x => x.OrganisationId == userService.UserService.OrganisationId)
                .Where(x => x.UserId == userService.UserService.UserId)
                .Select(x => OrganisationRoles.FromId(x.RoleId))
                .FirstOrDefault();

            var org = userService.Organisation;
            var user = userService.User;
            var us = userService.UserService;

            var organisationDto = org != null ? new ServiceUserOrganisationDto {
                Id = org.Id,
                OrganisationId = org.Id,
                UserId = us.UserId,
                UserStatus = user.Status,
                UserCreatedAt = ToUtc(user.CreatedAt),
                UserUpdatedAt = ToUtc(user.UpdatedAt),
                CreatedAt = ToUtc(org.CreatedAt),
                UpdatedAt = ToUtc(org.UpdatedAt),
                Name = org.Name ?? string.Empty,
                Category = org.Category,
                Type = org.Type,
                Urn = org.Urn,
                Uid = org.Uid,
                Ukprn = org.Ukprn,
                EstablishmentNumber = org.EstablishmentNumber,
                Status = org.Status,
                ClosedOn = org.ClosedOn,
                Address = org.Address,
                PhaseOfEducation = org.PhaseOfEducation,
                StatutoryLowAge = org.StatutoryLowAge,
                StatutoryHighAge = org.StatutoryHighAge,
                Telephone = org.Telephone,
                RegionCode = org.RegionCode,
                LegacyId = org.LegacyId.ToString(),
                CompanyRegistrationNumber = org.CompanyRegistrationNumber,
                DistrictAdministrative_name = org.DistrictAdministrativeName,
                MasteringCode = org.MasteringCode,
                ProviderProfileID = org.ProviderProfileId,
                SourceSystem = org.SourceSystem,
                UPIN = org.Upin,
                ProviderTypeName = org.ProviderTypeName,
                GIASProviderType = org.GiasProviderType,
                PIMSProviderType = org.PimsProviderType,
                ProviderTypeCode = org.ProviderTypeCode,
                PIMSProviderTypeCode = org.PimsProviderTypeCode,
                PIMSStatus = org.PimsStatus,
                OpenedOn = org.OpenedOn,
                DistrictAdministrativeName = org.DistrictAdministrativeName1,
                DistrictAdministrativeCode = org.DistrictAdministrativeCode,
                DistrictAdministrative_code = org.DistrictAdministrativeCode1,
                PIMSStatusName = org.PimsStatusName,
                GIASStatus = org.GiasStatus,
                GIASStatusName = org.GiasStatusName,
                MasterProviderStatusCode = org.MasterProviderStatusCode,
                MasterProviderStatusName = org.MasterProviderStatusName,
                LegalName = org.LegalName,
                IsOnAPAR = org.IsOnApar,
                LocalAuthorityId = org.LocalAuthorityId,
                LocalAuthorityCode = org.LocalAuthorityCode,
                LocalAuthorityName = org.LocalAuthorityName
            } : null;

            var dto = new ServiceUserDto {
                ApprovedAt = ToUtc(user.CreatedAt),
                UpdatedAt = ToUtc(user.UpdatedAt),
                Organisation = organisationDto,
                Roles = userServiceRolesForOrganisation,
                RoleName = userOrganisationRole?.Name ?? string.Empty,
                RoleId = userOrganisationRole?.Id ?? null,
                UserId = us.UserId,
                Email = user.Email ?? string.Empty,
                FamilyName = user.LastName ?? string.Empty,
                GivenName = user.FirstName ?? string.Empty,
                UserStatus = user.Status
            };
            return dto;
        }).ToList();

        if (userIds.Count > 0 && mappedUsers.All(u => string.IsNullOrEmpty(u.Email))) {
            return new GetServiceUsersResponse {
                Users = [],
                NumberOfRecords = 0,
                Page = pageNumber,
                NumberOfPages = 0
            };
        }

        return new GetServiceUsersResponse {
            Users = mappedUsers,
            NumberOfRecords = totalRecords,
            Page = pageNumber,
            NumberOfPages = totalPages
        };
    }

    private static DateTime ToUtc(DateTime date)
    {
        return date.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();
    }

    //private static DateTime? ToUtc(DateTime? date)
    //{
    //    if (!date.HasValue)
    //        return null;
    //    return ToUtc(date.Value);
    //}
}
