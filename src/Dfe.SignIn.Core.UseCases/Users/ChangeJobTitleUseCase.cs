using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// An interactor to change the job title of a user.
/// </summary>
public sealed class ChangeJobTitleUseCase(
    IUnitOfWorkDirectories unitOfWork,
    IInteractionDispatcher interaction
) : Interactor<ChangeJobTitleRequest, ChangeJobTitleResponse>
{
    /// <inheritdoc/>
    public override async Task<ChangeJobTitleResponse> InvokeAsync(
        InteractionContext<ChangeJobTitleRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var user = await unitOfWork.Repository<UserEntity>()
            .Where(x => x.Sub == context.Request.UserId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw UserNotFoundException.FromUserId(context.Request.UserId);

        if (user.JobTitle == context.Request.NewJobTitle) {
            return new ChangeJobTitleResponse();
        }

        user.JobTitle = context.Request.NewJobTitle;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await interaction.DispatchAsync(
            new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeJobTitle,
                Message = $"Successfully changed job title to {context.Request.NewJobTitle}",
                UserId = context.Request.UserId,
            }
        );

        return new ChangeJobTitleResponse();
    }
}
