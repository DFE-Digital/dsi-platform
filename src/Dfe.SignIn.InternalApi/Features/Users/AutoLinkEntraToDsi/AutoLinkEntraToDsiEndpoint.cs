using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.Messaging;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Configuration;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Models;
using Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Services;
using Dfe.SignIn.InternalApi.Services.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi;

/// <summary>
/// An endpoint to link the Entra user to a DSI
///
/// This endpoint carries a workflow.
///
/// 1. if the user exists in the DSI database and can be found by the EntraId
/// then its returned
///
/// 2. Invoked when the user doesn't exist via the entraId and attempts to locate
/// it by the email address. This is typically used for migration or support purposes
///
/// 3. User does not exist in DSI and thus gets created.
/// </summary>
/// <param name="directoriesDbContext">The database peristence layer</param>
/// <param name="auditWriter">The audit writer used for auditing</param>
/// <param name="timeProvider">The ability to resolve the current system time</param>
/// <param name="removeInviteService">Logic for removing a user invitation</param>
/// <param name="genericEmailCheck">Guard rails to restrict email addresses used.</param>
/// <param name="userCreator">A creator for creating the user</param>
/// <param name="eventPublisher">An event publisher</param>
/// <param name="notificationSettings">Application specific settings for notifications</param>
/// <param name="logger">A generic logger for logging application level messages</param>
public sealed class AutoLinkEntraToDsiEndpoint(
    DbDirectoriesContext directoriesDbContext,
    IAuditWriter auditWriter,
    TimeProvider timeProvider,
    RemoveInviteService removeInviteService,
    GenericEmailCheck genericEmailCheck,
    IUserCreator userCreator,
    IEventPublisher eventPublisher,
    IOptions<NotificationSettings> notificationSettings,
    ILogger<AutoLinkEntraToDsiEndpoint> logger) : IEndpoint
{
    /// <summary>
    ///   /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.AutoLinkEntraToDsi, (
            [FromBody] AutoLinkEntraUserToDsiRequest request,
            [FromServices] AutoLinkEntraToDsiEndpoint endpoint,
            CancellationToken cancellationToken) =>
            endpoint.Handle(request, cancellationToken))
            .WithName("Associates the Entra account to a Dsi")
            .WithTags("Users")
            .WithStandardResponses();
    }

    /// <summary>
    /// Links the Entra application user to a DSI user.
    /// </summary>
    /// <param name="request">The request containing the information needed to carry out the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<AutoLinkEntraUserToDsiResponse> Handle(AutoLinkEntraUserToDsiRequest request, CancellationToken cancellationToken)
    {
        var UserId = await this.GetExistingLinkedUserAsync(request, cancellationToken)
            ?? await this.LinkToExistingDsiUserAsync(request, cancellationToken)
            ?? await this.CreateDsiUserAsync(request, cancellationToken);

        return new AutoLinkEntraUserToDsiResponse {
            UserId = UserId
        };
    }

    private async Task<Guid?> GetExistingLinkedUserAsync(AutoLinkEntraUserToDsiRequest request,
        CancellationToken cancellationToken)
    {
        var user = await this.GetUserQuery(request)
            .Select(x => new { x.Sub, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            return null;
        }

        // User exists in the system; are they an active user though?
        this.ValidateActiveUser((AccountStatus)user.Status);

        return user.Sub;
    }

    private async Task<Guid?> LinkToExistingDsiUserAsync(AutoLinkEntraUserToDsiRequest request, CancellationToken cancellationToken)
    {
        // Let's try to associate the Entra user object with a DSI account.

        var user = await directoriesDbContext.Users.Where(x => x.Email == request.EmailAddress)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) {
            return null;
        }

        // User exists in the system; are they an active user though?
        this.ValidateActiveUser((AccountStatus)user.Status);

        // Check if user is already linked to an Entra account; but a different one
        if (user.EntraOid is Guid userEntraOid && userEntraOid != request.EntraUserId) {
            throw UserAlreadyLinkedToEntraAccountException.FromUserIds(
                user.Sub, userEntraOid, request.EntraUserId!.Value);
        }

        var existingEntraUser = await directoriesDbContext.Users
            .Where(x => x.EntraOid == request.EntraUserId)
            .Select(x => new { x.Sub })
            .FirstOrDefaultAsync(cancellationToken);

        // Check if the target Entra Oid has already been linked to a different user
        //TODO : This logic will never get invoked!!
        if (existingEntraUser is not null && existingEntraUser.Sub != user.Sub) {
            throw EntraAccountAlreadyLinkedToDifferentUserException.FromUserIds(
                user.Sub, request.EntraUserId!.Value, existingEntraUser.Sub);
        }

        bool nameUpdated = false;
        bool entraAccountLinked = false;

        if (user.FirstName != request.FirstName) {
            user.FirstName = request.FirstName;
            nameUpdated = true;
        }
        if (user.LastName != request.LastName) {
            user.LastName = request.LastName;
            nameUpdated = true;
        }
        if (user.EntraOid is null) {
            user.EntraOid = request.EntraUserId;
            user.IsEntra = true;
            user.EntraLinked = timeProvider.GetUtcNow().UtcDateTime;
            entraAccountLinked = true;
        }
        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        if (nameUpdated) {
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeName,
                Message = $"Successfully changed name to {user.FirstName} {user.LastName}",
                UserId = user.Sub,
            });
        }

        if (entraAccountLinked) {
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.Auth,
                EventName = AuditAuthEventNames.LinkToExistingUser,
                Message = $"Linked Entra account with existing DfE Sign-In user {user.Email}.",
                UserId = user.Sub
            });
        }

        return user.Sub;
    }
    private async Task<Guid> CreateDsiUserAsync(AutoLinkEntraUserToDsiRequest request, CancellationToken cancellationToken)
    {
        var pendingInvite = await directoriesDbContext.Invitations
            .Where(x => x.Email == request.EmailAddress && !x.Completed)
            .FirstOrDefaultAsync(cancellationToken);

        //TODO What do we do if pending invite cannot be found???

        if (pendingInvite is not null && pendingInvite.Uid is not null) {
            await auditWriter.Log(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.Auth,
                EventName = AuditAuthEventNames.LinkToInvitedUser,
                Message = $"Linked Entra account with pending DfE Sign-In invitation {request.EmailAddress}",
                UserId = pendingInvite.Uid
            });

            return pendingInvite.Uid.Value;
        }

        var newUserResponse = await userCreator.CreateAsync(new User {
            EntraOid = request.EntraUserId!.Value,
            Username = request.EmailAddress,
            FirstName = request.FirstName,
            LastName = request.LastName
        }, cancellationToken);

        pendingInvite.Completed = true;
        pendingInvite.Uid = newUserResponse.Sub;

        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        var allStaleInvitations = await directoriesDbContext.Invitations
            .Where(x => x.Email == request.EmailAddress
            && x.Id != pendingInvite.Id && !x.Completed)
            .ToListAsync(cancellationToken);

        foreach (var staleInvite in allStaleInvitations) {
            logger.LogInformation("Deleting stale invitation {staleInviteId} for email {staleInviteEmail} following completion of invitation ${invId}",
               staleInvite.Id, staleInvite.Email, pendingInvite.Id);

            directoriesDbContext.Invitations.Remove(staleInvite);
            await directoriesDbContext.SaveChangesAsync(cancellationToken);

            await removeInviteService.Handle(newUserResponse.Sub, staleInvite.Id);
        }

        var isGenericEmail = genericEmailCheck.IsEmailGeneric(newUserResponse.Email);

        if (isGenericEmail) {
            await eventPublisher.PublishAsync(new SupportRequestEvent {
                Email = notificationSettings.Value.SupportTeamEmail,
                Type = "potential-generic-email-address",
                TypeAdditionalInfo = null,
                Message = $"New user has a potentially generic email address, please review the user: ${pendingInvite.Email} ({pendingInvite.FirstName} {pendingInvite.LastName})."
            });
        }

        await auditWriter.Log(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.Auth,
            EventName = AuditAuthEventNames.LinkToNewUser,
            Message = $"Linked Entra account with new DfE Sign-In user {request.EmailAddress}",
            UserId = newUserResponse?.Sub
        });

        return newUserResponse!.Sub;
    }

    private void ValidateActiveUser(AccountStatus status)
    {
        if (status != AccountStatus.Active) {
            throw new CannotLinkInactiveUserException();
        }
    }
    private IQueryable<UserEntity> GetUserQuery(AutoLinkEntraUserToDsiRequest request)
    {
        return request.EntraUserId.HasValue
        ? directoriesDbContext.Users.Where(x => x.EntraOid == request.EntraUserId)
        : directoriesDbContext.Users.Where(x => x.Email == request.EmailAddress);
    }
}
