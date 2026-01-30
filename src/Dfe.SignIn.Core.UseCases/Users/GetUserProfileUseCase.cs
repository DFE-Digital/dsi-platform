using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for getting the profile of a user.
/// </summary>
public sealed class GetUserProfileUseCase(
    IUnitOfWorkDirectories unitOfWork
) : Interactor<GetUserProfileRequest, GetUserProfileResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserProfileResponse> InvokeAsync(
        InteractionContext<GetUserProfileRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var user = await unitOfWork.Repository<UserEntity>()
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
        };
    }
}
