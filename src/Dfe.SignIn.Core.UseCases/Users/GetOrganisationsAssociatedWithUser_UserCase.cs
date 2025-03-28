using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Users.Interactions;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for getting all of the organisations that are associated with a particular user.
/// </summary>
public sealed class GetOrganisationsAssociatedWithUser_UseCase
    : IInteractor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>
{
    /// <inheritdoc/>
    public Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(
        GetOrganisationsAssociatedWithUserRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
