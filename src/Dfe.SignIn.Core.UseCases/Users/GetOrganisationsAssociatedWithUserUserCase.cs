using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.Users.Interactions;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for getting all of the organisations that are associated with a particular user.
/// </summary>
public sealed class GetOrganisationsAssociatedWithUserUseCase
    : Interactor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>
{
    /// <inheritdoc/>
    public override Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(
        InteractionContext<GetOrganisationsAssociatedWithUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        throw new NotImplementedException();
    }
}
