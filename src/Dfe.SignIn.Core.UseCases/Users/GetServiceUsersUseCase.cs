using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Extensions;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Get the service users for a given service (client).
/// </summary>
/// <param name="uowOrganisations"></param>
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

        var query = uowOrganisations.Repository<UserServiceEntity>()
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Organisation)
            .Where(x => x.ServiceId == applicationId)
            .Where(x => x.User != null)
            .Where(x => x.Organisation != null);

        // Apply optional filters
        if (context.Request.UserStatus.HasValue) {
            query = query.Where(x => x.Status == (short)context.Request.UserStatus.Value);
        }

        if (context.Request.DateFrom.HasValue) {
            query = query.Where(x => x.CreatedAt >= context.Request.DateFrom.Value);
        }

        if (context.Request.DateTo.HasValue) {
            query = query.Where(x => x.CreatedAt <= context.Request.DateTo.Value);
        }

        var totalRecords = await query.CountAsync(cancellationToken);
        if (totalRecords == 0) {
            return GetServiceUsersResponse.Empty(pageNumber);
        }

        var pagedEntities = await query
            .OrderBy(x => x.UserId)
            .ThenBy(x => x.OrganisationId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var userIds = pagedEntities.Select(us => us.UserId).Distinct().ToList();

        var rolesLookup = await this.GetServiceRolesLookupAsync(applicationId, userIds, cancellationToken);
        var orgRolesLookup = await this.GetOrgRolesLookupAsync(userIds, cancellationToken);

        var mappedUsers = pagedEntities.Select(entity =>
            MapToServiceUserDto(entity, rolesLookup, orgRolesLookup)).ToList();

        // Added to make sure it is backwards compatible with existing data where email might be missing,
        // and to prevent potential issues in the UI or other layers that expect an email address.
        if (mappedUsers.All(u => string.IsNullOrEmpty(u.Email))) {
            return GetServiceUsersResponse.Empty(pageNumber);
        }

        return GetServiceUsersResponse.FromUsers(
            mappedUsers,
            totalRecords,
            pageNumber,
            pageSize
        );
    }

    private async Task<ILookup<(Guid UserId, Guid OrgId), ServiceUserRoleDto>> GetServiceRolesLookupAsync(
        Guid appId, List<Guid> userIds, CancellationToken ct)
    {
        var roles = await uowOrganisations.Repository<UserServiceRoleEntity>()
            .AsNoTracking()
            .Include(x => x.Role)
            .Where(x => x.ServiceId == appId)
            .Where(x => userIds.Contains(x.UserId))
            .ToListAsync(ct);

        return roles.ToLookup(
            key => (key.UserId, key.OrganisationId),
            val => new ServiceUserRoleDto {
                Id = val.Role?.Id.ToString() ?? string.Empty,
                Name = val.Role?.Name ?? string.Empty,
                Code = val.Role?.Code ?? string.Empty,
                NumericId = val.Role?.NumericId.ToString() ?? string.Empty,
                Status = val.Role?.Status ?? (int)ApplicationRoleStatus.Inactive
            });
    }

    private async Task<ILookup<Guid, (Guid OrgId, short RoleId)>> GetOrgRolesLookupAsync(
        List<Guid> userIds, CancellationToken ct)
    {
        var orgRoles = await uowOrganisations.Repository<UserOrganisationEntity>()
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .Select(x => new { x.UserId, x.OrganisationId, x.RoleId })
            .ToListAsync(ct);

        return orgRoles.ToLookup(x => x.UserId, x => (x.OrganisationId, x.RoleId));
    }

    private static ServiceUserDto MapToServiceUserDto(
        UserServiceEntity entity,
        ILookup<(Guid, Guid), ServiceUserRoleDto> rolesLookup,
        ILookup<Guid, (Guid OrgId, short RoleId)> orgRolesLookup)
    {
        var user = entity.User;
        var org = entity.Organisation;

        var orgRoleId = orgRolesLookup[entity.UserId]
            .Where(x => x.OrgId == entity.OrganisationId)
            .Select(x => (short?)x.RoleId)
            .FirstOrDefault();

        var userOrgRole = orgRoleId != null ? OrganisationRoles.FromId(orgRoleId.Value) : null;

        var userRoles = rolesLookup[(entity.UserId, entity.OrganisationId ?? Guid.Empty)].ToList();

        return new ServiceUserDto {
            UserId = entity.UserId,
            Email = user?.Email ?? string.Empty,
            FamilyName = user?.LastName ?? string.Empty,
            GivenName = user?.FirstName ?? string.Empty,
            UserStatus = user?.Status ?? 0,
            ApprovedAt = user?.CreatedAt.ToUtc(),
            UpdatedAt = user?.UpdatedAt.ToUtc(),
            RoleName = userOrgRole?.Name ?? string.Empty,
            RoleId = userOrgRole?.Id,
            Roles = userRoles,
            Organisation = MapOrganisationDto(org, entity.UserId, user)
        };
    }

    private static ServiceUserOrganisationDto? MapOrganisationDto(OrganisationEntity? org, Guid userId, UserEntity? user)
    {
        if (org == null) {
            return null;
        }

        return new ServiceUserOrganisationDto {
            Id = org.Id,
            OrganisationId = org.Id,
            UserId = userId,
            UserStatus = user?.Status ?? 0,
            UserCreatedAt = user?.CreatedAt.ToUtc(),
            UserUpdatedAt = user?.UpdatedAt.ToUtc(),
            CreatedAt = org.CreatedAt.ToUtc(),
            UpdatedAt = org.UpdatedAt.ToUtc(),
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
            DistrictAdministrativeNameAlt = org.DistrictAdministrativeName,
            MasteringCode = org.MasteringCode,
            ProviderProfileID = org.ProviderProfileId,
            SourceSystem = org.SourceSystem,
            Upin = org.Upin,
            ProviderTypeName = org.ProviderTypeName,
            GiasProviderType = org.GiasProviderType,
            PimsProviderType = org.PimsProviderType,
            ProviderTypeCode = org.ProviderTypeCode,
            PimsProviderTypeCode = org.PimsProviderTypeCode,
            PimsStatus = org.PimsStatus,
            OpenedOn = org.OpenedOn,
            DistrictAdministrativeName = org.DistrictAdministrativeName1,
            DistrictAdministrativeCode = org.DistrictAdministrativeCode,
            DistrictAdministrativeCodeAlt = org.DistrictAdministrativeCode1,
            PimsStatusName = org.PimsStatusName,
            GiasStatus = org.GiasStatus,
            GiasStatusName = org.GiasStatusName,
            MasterProviderStatusCode = org.MasterProviderStatusCode,
            MasterProviderStatusName = org.MasterProviderStatusName,
            LegalName = org.LegalName,
            IsOnAPAR = org.IsOnApar,
            LocalAuthorityId = org.LocalAuthorityId,
            LocalAuthorityCode = org.LocalAuthorityCode,
            LocalAuthorityName = org.LocalAuthorityName
        };
    }
}
