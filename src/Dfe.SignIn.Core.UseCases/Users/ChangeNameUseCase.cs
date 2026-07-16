using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// An interactor to change the job title of a user.
/// </summary>
public sealed class ChangeNameUseCase(
    DbDirectoriesContext directoriesDbContext,
    IInteractionDispatcher interaction
) : Interactor<ChangeNameRequest, ChangeNameResponse>
{
    /// <inheritdoc/>
    public override async Task<ChangeNameResponse> InvokeAsync(
        InteractionContext<ChangeNameRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == context.Request.UserId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw UserNotFoundException.FromUserId(context.Request.UserId);

        if (user.FirstName == context.Request.FirstName && user.LastName == context.Request.LastName) {
            return new ChangeNameResponse();
        }

        if (user.FirstName != context.Request.FirstName) {
            var normalisedFirstName = context.Request.FirstName.NormalizeWhitespace();
            user.FirstName = normalisedFirstName;
        }

        if (user.LastName != context.Request.LastName) {
            var normalisedLastName = context.Request.LastName.NormalizeWhitespace();
            user.LastName = normalisedLastName;
        }

        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        await interaction.DispatchAsync(
            new WriteToAuditRequest {
                EventCategory = AuditEventCategoryNames.ChangeJobTitle,
                Message = $"Successfully changed users name to {user.FirstName} {user.LastName}",
                UserId = context.Request.UserId,
            }
        );

        return new ChangeNameResponse();
    }
}
