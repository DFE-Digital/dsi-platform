using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for linking an Entra user to a DfE Sign-in user.
/// </summary>
public sealed class LinkEntraUserToDsiUseCase(
    IUnitOfWorkDirectories unitOfWork,
    IInteractionDispatcher interaction,
    TimeProvider timeProvider
    ) : Interactor<LinkEntraUserToDsiRequest, LinkEntraUserToDsiResponse>
{
    /// <inheritdoc/>
    public override async Task<LinkEntraUserToDsiResponse> InvokeAsync(
        InteractionContext<LinkEntraUserToDsiRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var user = await unitOfWork.Repository<UserEntity>()
            .Where(x => x.Sub == context.Request.DsiUserId)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw UserNotFoundException.FromUserId(context.Request.DsiUserId);

        // Check if user is already linked to an Entra account; but a different one
        if (user.EntraOid is Guid userEntraOid && userEntraOid != context.Request.EntraUserId) {
            throw UserAlreadyLinkedToEntraAccountException.FromUserIds(
                user.Sub, userEntraOid, context.Request.EntraUserId);
        }

        var existingEntraUser = await unitOfWork
            .Repository<UserEntity>()
            .Where(x => x.EntraOid == context.Request.EntraUserId)
            .Select(x => new { x.Sub })
            .FirstOrDefaultAsync(cancellationToken);

        // Check if the target Entra Oid has already been linked to a different user
        if (existingEntraUser is not null && existingEntraUser.Sub != user.Sub) {
            throw EntraAccountAlreadyLinkedToDifferentUserException.FromUserIds(
                user.Sub, context.Request.EntraUserId, existingEntraUser.Sub);
        }

        bool nameUpdated = false;
        bool entraAccountLinked = false;

        if (user.FirstName != context.Request.FirstName) {
            user.FirstName = context.Request.FirstName;
            nameUpdated = true;
        }
        if (user.LastName != context.Request.LastName) {
            user.LastName = context.Request.LastName;
            nameUpdated = true;
        }
        if (user.EntraOid is null) {
            user.EntraOid = context.Request.EntraUserId;
            user.IsEntra = true;
            user.EntraLinked = timeProvider.GetUtcNow().UtcDateTime;
            entraAccountLinked = true;
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (nameUpdated) {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeName,
                Message = $"Successfully changed name to {user.FirstName} {user.LastName}",
                UserId = user.Sub,
            });
        }

        if (entraAccountLinked) {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.Auth,
                EventName = AuditAuthEventNames.LinkToExistingUser,
                Message = $"Linked Entra account with existing DfE Sign-In user {user.Email}.",
                UserId = user.Sub,
            });
        }

        return new LinkEntraUserToDsiResponse();
    }
}
