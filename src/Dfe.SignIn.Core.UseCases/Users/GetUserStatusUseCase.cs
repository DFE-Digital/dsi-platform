using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// An interactor to determine if a user exists and retrieve their account status.
/// </summary>
public sealed class GetUserStatusUseCase(
    IUnitOfWorkDirectories unitOfWork
) : Interactor<GetUserStatusRequest, GetUserStatusResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserStatusResponse> InvokeAsync(
        InteractionContext<GetUserStatusRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var queryBuilder = unitOfWork.Repository<UserEntity>();
        queryBuilder = context.Request.EntraUserId.HasValue
            ? queryBuilder.Where(x => x.EntraOid == context.Request.EntraUserId.Value)
            : queryBuilder.Where(x => x.Email == context.Request.EmailAddress);

        var user = await queryBuilder.Select(x => new { x.Sub, x.Status }).FirstOrDefaultAsync(cancellationToken);

        return new GetUserStatusResponse {
            UserExists = user is not null,
            UserId = user?.Sub,
            AccountStatus = user is not null ? EnumHelpers.MapEnum<AccountStatus>((int)user.Status) : null
        };
    }
}
