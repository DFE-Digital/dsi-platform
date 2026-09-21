using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
/// <param name="logger">A generic logger for logging application level messages</param>
public sealed class AutoLinkEntraToDsiEndpoint(
    DbDirectoriesContext directoriesDbContext,
    IAuditWriter auditWriter,
    TimeProvider timeProvider,
    IInteractionDispatcher interaction,
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
            .WithStandardResponses<AutoLinkEntraUserToDsiResponse>();
    }

    /// <summary>
    /// Links the Entra application user to a DSI user.
    /// </summary>
    /// <param name="request">The request containing the information needed to carry out the action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<AutoLinkEntraUserToDsiResponse> Handle(AutoLinkEntraUserToDsiRequest request, CancellationToken cancellationToken)
    {
        var userId = await this.GetExistingLinkedUserAsync(request, cancellationToken);

#pragma warning disable IDE0074 // Use compound assignment
        if (userId is null) {
            userId = await this.LinkToExistingDsiUserAsync(request, cancellationToken);
        }

        if (userId is null) {
            userId = await this.CreateDsiUserAsync(new AutoLinkEntraUserToDsiRequest {
                EmailAddress = request.EmailAddress,
                EntraUserId = request.EntraUserId,
                FirstName = request.FirstName,
                LastName = request.LastName
            });
        }
#pragma warning restore IDE0074 // Use compound assignment

        return new AutoLinkEntraUserToDsiResponse {
            UserId = userId.Value
        };
    }

    private async Task<Guid?> GetExistingLinkedUserAsync(AutoLinkEntraUserToDsiRequest request,
        CancellationToken cancellationToken)
    {
        var user = await directoriesDbContext.Users
            .Where(x => x.EntraOid == request.EntraUserId)
            .Select(x => new { x.Sub, x.Status })
            .FirstOrDefaultAsync(cancellationToken);

        // User cannot be found via EntraId.
        if (user is null) {
            return null;
        }

        // User exists in the system; are they an active user though?
        ValidateActiveUser((AccountStatus)user.Status);

        return user.Sub;
    }

    private async Task<Guid?> LinkToExistingDsiUserAsync(AutoLinkEntraUserToDsiRequest request, CancellationToken cancellationToken)
    {
        // Let's try to associate the Entra user object with a DSI account.
        var user = await directoriesDbContext.Users.Where(x => x.Email == request.EmailAddress)
            .FirstOrDefaultAsync(cancellationToken);

        // User cannot be found via email address.
        if (user is null) {
            return null;
        }

        // User exists in the system; are they an active user though?
        ValidateActiveUser((AccountStatus)user.Status);

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
        if (existingEntraUser is not null && existingEntraUser.Sub != user.Sub) {
            throw EntraAccountAlreadyLinkedToDifferentUserException.FromUserIds(
                user.Sub, request.EntraUserId!.Value, existingEntraUser.Sub);
        }

        var (nameUpdated, entraAccountLinked) = await this.UpdateUserAsync(user, request, cancellationToken);

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

    private async Task<(bool nameUpdated, bool entraAccountLinked)> UpdateUserAsync(UserEntity user, AutoLinkEntraUserToDsiRequest request, CancellationToken cancellationToken)
    {
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

        return (nameUpdated, entraAccountLinked);
    }

    private async Task<Guid> CreateDsiUserAsync(AutoLinkEntraUserToDsiRequest request)
    {
        // User does not exist in the system; is there a pending invitation?
        var completeAnyPendingInvitationResponse = await interaction.DispatchAsync(
            new CompleteAnyPendingInvitationRequest {
                EmailAddress = request.EmailAddress,
                EntraUserId = request.EntraUserId!.Value,
            }
        ).To<CompleteAnyPendingInvitationResponse>();

        if (completeAnyPendingInvitationResponse.UserId is not null) {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.Auth,
                EventName = AuditAuthEventNames.LinkToInvitedUser,
                Message = $"Linked Entra account with pending DfE Sign-In invitation {request.EmailAddress}",
                UserId = completeAnyPendingInvitationResponse.UserId,
            });

            return completeAnyPendingInvitationResponse.UserId.Value;
        }

        // Create new user in system and link to the associated Entra user.
        var createUserResponse = await interaction.DispatchAsync(
            new CreateUserRequest {
                EntraUserId = request.EntraUserId!.Value,
                EmailAddress = request.EmailAddress,
                FirstName = request.FirstName,
                LastName = request.LastName,
            }
        ).To<CreateUserResponse>();

        await interaction.DispatchAsync(new WriteToAuditRequest {
            EventCategory = AuditEventCategoryNames.Auth,
            EventName = AuditAuthEventNames.LinkToNewUser,
            Message = $"Linked Entra account with new DfE Sign-In user {request.EmailAddress}",
            UserId = createUserResponse.UserId,
        });

        return createUserResponse.UserId;
    }

    private static void ValidateActiveUser(AccountStatus status)
    {
        if (status != AccountStatus.Active) {
            throw new CannotLinkInactiveUserException();
        }
    }
}
