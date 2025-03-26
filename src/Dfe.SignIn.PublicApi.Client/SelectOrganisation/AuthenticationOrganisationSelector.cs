using Dfe.SignIn.Core.Framework;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Represents a service that initiates organisation selection for an authenticated user.
/// </summary>
public interface IAuthenticationOrganisationSelector
{
    /// <summary>
    /// Initiate organisation selection for an authenticated user.
    /// </summary>
    /// <remarks>
    ///   <para>The organisation claim of the user is changed once the user has selected
    ///   an organisation through this mechanism.</para>
    ///   <para>Consider using the select organisation endpoint directly if the select
    ///   organisation feature is required for an alternative purpose.</para>
    /// </remarks>
    /// <param name="context">The context of the current HTTP request.</param>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the user is not currently authenticated.</para>
    /// </exception>
    Task InitiateSelectionAsync(HttpContext context);
}

/// <summary>
/// Concrete implementation of <see cref="IAuthenticationOrganisationSelector"/>.
/// </summary>
public sealed class AuthenticationOrganisationSelector(
    IOptions<AuthenticationOrganisationSelectorOptions> optionsAccessor,
    IInteractor<CreateSelectOrganisationSession_PublicApiRequest, CreateSelectOrganisationSession_PublicApiResponse> createSelectOrganisationSession,
    IOrganisationClaimManager organisationClaimManager
) : IAuthenticationOrganisationSelector
{
    /// <inheritdoc/>
    public async Task InitiateSelectionAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        CheckUserIsAuthenticated(context);

        var request = this.PrepareRequest(context);

        var selectOrganisationResponse = await createSelectOrganisationSession.InvokeAsync(request);

        if (selectOrganisationResponse.HasOptions) {
            context.Response.Redirect(selectOrganisationResponse.Url.ToString());
        }
        else {
            await organisationClaimManager.UpdateOrganisationClaimAsync(context, "null");
            var options = optionsAccessor.Value;
            context.Response.Redirect(options.CompletedPath);
        }
    }

    private static void CheckUserIsAuthenticated(HttpContext context)
    {
        bool isUserAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        if (!isUserAuthenticated) {
            throw new InvalidOperationException("Cannot initiate organisation selection since user is not authenticated.");
        }
    }

    private CreateSelectOrganisationSession_PublicApiRequest PrepareRequest(HttpContext context)
    {
        var options = optionsAccessor.Value;

        var request = new CreateSelectOrganisationSession_PublicApiRequest {
            UserId = context.User.GetDsiUserId(),
            CallbackUrl = new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{options.SelectOrganisationCallbackPath}"),
        };
        if (options.PrepareSelectOrganisationRequest is not null) {
            request = options.PrepareSelectOrganisationRequest(request);
        }
        return request;
    }
}
