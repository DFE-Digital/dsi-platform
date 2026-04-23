using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for retrieving all service users with pagination (no filters).
/// </summary>
public sealed class GetServiceUsersUseCase : Interactor<GetServiceUsersRequest, GetServiceUsersResponse>
{
    /// <summary>
    /// Handles the request to retrieve service users with pagination.
    /// </summary>
    /// <param name="context">The interaction context containing the request and metadata.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="GetServiceUsersResponse"/> containing the paginated list of service users.</returns>
    public override async Task<GetServiceUsersResponse> InvokeAsync(
        InteractionContext<GetServiceUsersRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        // TODO: Implement data fetching logic for all service users (no filters, just pagination)
        // This is a stub for now

        await Task.Delay(100); // Simulate async work

        return new GetServiceUsersResponse {
            Users = [],
            NumberOfRecords = 0,
            Page = context.Request.PageNumber,
            NumberOfPages = 0
        };
    }
}
