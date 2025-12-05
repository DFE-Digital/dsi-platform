using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.WebFramework.Mvc.Features;

/// <summary>
/// Provides access to the user profile of the authenticated user for the current request.
/// </summary>
public interface IUserProfileFeature
{
    /// <summary>
    /// Gets or sets the ID of the user in DfE Sign-In.
    /// </summary>
    Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates if the user is associated with an
    /// external account.
    /// </summary>
    bool IsEntra { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates if the user is an internal team member.
    /// </summary>
    bool IsInternalUser { get; set; }

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    string GivenName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    string Surname { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    string EmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the job title of the user.
    /// </summary>
    /// <value>
    ///   <para>A value of null indicates that no job title has been provided.</para>
    /// </value>
    string? JobTitle { get; set; }
}

/// <inheritdoc cref="IUserProfileFeature"/>
public sealed class UserProfileFeature : IUserProfileFeature
{
    /// <inheritdoc/>
    public required Guid UserId { get; set; }

    /// <inheritdoc/>
    public required bool IsEntra { get; set; }

    /// <inheritdoc/>
    public required bool IsInternalUser { get; set; }

    /// <inheritdoc/>
    public required string GivenName { get; set; }

    /// <inheritdoc/>
    public required string Surname { get; set; }

    /// <inheritdoc/>
    public required string EmailAddress { get; set; }

    /// <inheritdoc/>
    public required string? JobTitle { get; set; }
}

/// <summary>
/// Middleware which retrieves the profile of an authenticated user which is then
/// provided for the current request with <see cref="IUserProfileFeature"/>.
/// </summary>
public sealed class UserProfileMiddleware(
    IInteractionDispatcher interaction,
    RequestDelegate next)
{
    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">Context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var userProfileFeature = context.Features.Get<IUserProfileFeature>();
        if (userProfileFeature is not null) {
            return;
        }

        if (context.User?.Identity?.IsAuthenticated == true) {
            Guid userId = context.User.GetUserId();

            var profileResponse = await interaction.DispatchAsync(
                new GetUserProfileRequest { UserId = userId }
            ).To<GetUserProfileResponse>();

            context.Features.Set<IUserProfileFeature>(new UserProfileFeature {
                UserId = userId,
                IsEntra = profileResponse.IsEntra,
                IsInternalUser = profileResponse.IsInternalUser,
                GivenName = profileResponse.GivenName,
                Surname = profileResponse.Surname,
                EmailAddress = profileResponse.EmailAddress,
                JobTitle = profileResponse.JobTitle,
            });
        }

        await next(context);
    }
}
