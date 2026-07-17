using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for getting the profile of a user.
/// </summary>
public sealed class GetUserProfileUseCase(
    DbDirectoriesContext directoriesDbContext
) : Interactor<GetUserProfileRequest, GetUserProfileResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserProfileResponse> InvokeAsync(
        InteractionContext<GetUserProfileRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == context.Request.UserId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw UserNotFoundException.FromUserId(context.Request.UserId);

        return new GetUserProfileResponse {
            IsEntra = user.IsEntra,
            IsInternalUser = user.IsInternalUser,
            FirstName = user.FirstName,
            LastName = user.LastName,
            JobTitle = !string.IsNullOrWhiteSpace(user.JobTitle) ? user.JobTitle : null,
            EmailAddress = user.Email,
            Status = user.Status,
        };
    }
}
