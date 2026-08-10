using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Dfe.SignIn.Core.Contracts.Users;
using Refit;

namespace Dfe.SignIn.Core.Contracts.Features.Users;

/// <summary>
/// Represents a client for interacting with user-related API endpoints.
/// </summary>
public interface IUsersApiClient
{
    /// <summary>
    /// Changes the name of a user based on the provided request.
    /// </summary>
    /// <param name="request">The request containing the user's new name information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Post(UsersApiRoutes.ChangeName)]
    Task ChangeName([Body] ChangeNameRequest request);

    /// <summary>
    /// Determins if the authenicated user is an approver.
    /// </summary>
    /// <param name="userId">The ID of the user to check for approver status.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the approver status.</returns>
    [Get(UsersApiRoutes.IsApprover)]
    Task<IsOrganisationApproverResponse> IsApprover(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of pending approval requests for the logged in approver
    /// </summary>
    /// <param name="userId">The ID of the user to check for approver status.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the number of pending approvals</returns>
    [Get(UsersApiRoutes.PendingApprovalCounter)]
    Task<PendingApprovalCountResponse> PendingApprovalCount(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the profile information of a user based on the provided request.
    /// </summary>
    /// <param name="userId">The ID of the user whose profile information is being requested.</param>
    /// <returns>A task representing the asynchronous operation, containing the user's profile information.</returns>
    [Get(UsersApiRoutes.GetUserProfile)]
    Task<GetUserProfileResponse> GetUserProfile(Guid userId);

    /// <summary>
    /// Changes the job title of a user based on the provided request.
    /// </summary>
    /// <param name="userId">The ID of the user whose job title is being changed.</param>
    /// <param name="request">The request containing the user's new job title information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Post(UsersApiRoutes.ChangeJobTitle)]
    Task ChangeJobTitle(Guid userId, [Body] ChangeJobTitleRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Initiates the process of changing a user's email address based on the provided request.
    /// </summary>
    /// <param name="userId">The ID of the user whose email address is being changed.</param>
    /// <param name="request">The request containing the user's new email address information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Post(UsersApiRoutes.InitiateChangeEmail)]
    Task InitiateChangeEmailAddress(Guid userId, [Body] InitiateChangeEmailAddressRequest request);
}
