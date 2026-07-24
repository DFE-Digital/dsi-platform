using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.Exceptions;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// An interactor to change the job title of a user.
/// </summary>
public sealed class ChangeJobTitleUseCase(
    DbDirectoriesContext directoriesDbContext,
    IInteractionDispatcher interaction
) : Interactor<ChangeJobTitleRequest, ChangeJobTitleResponse>
{
    /// <inheritdoc/>
    public override async Task<ChangeJobTitleResponse> InvokeAsync(
        InteractionContext<ChangeJobTitleRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == context.Request.UserId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw UserNotFoundException.FromUserId(context.Request.UserId);

        if (user.JobTitle == context.Request.NewJobTitle) {
            return new ChangeJobTitleResponse();
        }

        var normalisedJobTitle = context.Request.NewJobTitle.NormalizeWhitespace();

        user.JobTitle = normalisedJobTitle;

        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        await interaction.DispatchAsync(
            new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeJobTitle,
                Message = $"Successfully changed job title to {normalisedJobTitle}",
                UserId = context.Request.UserId,
            }
        );

        return new ChangeJobTitleResponse();
    }
}
