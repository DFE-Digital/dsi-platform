using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// An interactor for cancelling a pending change of email address request for a user.
/// </summary>
/// <param name="interaction">The interaction dispatcher for handling interactions.</param>
/// <param name="uowDirectories">Unit of work for accessing directory-related data.</param>
public sealed class CancelPendingChangeEmailAddressUseCase(
    IInteractionDispatcher interaction,
    IUnitOfWorkDirectories uowDirectories
) : Interactor<CancelPendingChangeEmailAddressRequest, CancelPendingChangeEmailAddressResponse>
{
    /// <inheritdoc/>
    public override async Task<CancelPendingChangeEmailAddressResponse> InvokeAsync(
    InteractionContext<CancelPendingChangeEmailAddressRequest> context,
    CancellationToken cancellationToken)
    {
        context.ThrowIfHasValidationErrors();

        var user = await uowDirectories.Repository<UserEntity>()
            .SingleOrDefaultAsync(x => x.Sub == context.Request.UserId, cancellationToken);

        var pendingCode = await uowDirectories.Repository<UserCodeEntity>()
            .SingleOrDefaultAsync(x => x.Uid == context.Request.UserId && x.CodeType == "changeemail", cancellationToken);

        if (pendingCode != null) {
            await interaction.DispatchAsync(new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeEmail,
                EventName = AuditChangeEmailEventNames.CancelChangeEmail,
                Message = $"Cancel change email request from {user?.Email} (id: {context.Request.UserId})",
                UserId = context.Request.UserId
            });

            uowDirectories.Remove(pendingCode);
            await uowDirectories.SaveChangesAsync(cancellationToken);
        }

        return new CancelPendingChangeEmailAddressResponse();
    }
}
