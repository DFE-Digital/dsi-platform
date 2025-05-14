using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Handles the various events that occur during the user flow where the user is
/// selecting the active organisation.
/// </summary>
/// <remarks>
///   <para>The default behaviour can be customised by subclassing <see cref="StandardSelectOrganisationEvents"/>
///   or by providing as custom <see cref="ISelectOrganisationEvents"/>
///   implementation.</para>
/// </remarks>
/// <param name="optionsAccessor">Provides access to the user flow options.</param>
/// <param name="activeOrganisationProvider">A provider that can get or set the actively
/// selected organisation for the user.</param>
/// <seealso cref="StandardSelectOrganisationMiddleware"/>
public class StandardSelectOrganisationEvents(
    IOptions<StandardSelectOrganisationUserFlowOptions> optionsAccessor,
    IActiveOrganisationProvider activeOrganisationProvider
) : ISelectOrganisationEvents
{
    /// <inheritdoc/>
    public virtual Task OnStartSelection(IHttpContext context, Uri selectionUri)
    {
        context.Response.Redirect(selectionUri.ToString());
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public virtual async Task OnCancelSelection(IHttpContext context)
    {
        var activeOrganisationState = await activeOrganisationProvider.GetActiveOrganisationStateAsync(context);
        if (activeOrganisationState is null) {
            // User has not selected an organisation yet; sign them out.
            this.RedirectToSignOutPath(context);
            return;
        }
        this.RedirectToCompletedPath(context);
    }

    /// <inheritdoc/>
    public virtual async Task OnConfirmSelection(IHttpContext context, OrganisationDetails? organisation)
    {
        await activeOrganisationProvider.SetActiveOrganisationAsync(context, organisation);
        this.RedirectToCompletedPath(context);
    }

    /// <inheritdoc/>
    public virtual Task OnError(IHttpContext context, SelectOrganisationErrorCode errorCode)
    {
        throw new SelectOrganisationCallbackErrorException(errorCode);
    }

    /// <inheritdoc/>
    public virtual Task OnSignOut(IHttpContext context)
    {
        this.RedirectToSignOutPath(context);
        return Task.CompletedTask;
    }

    private void RedirectToCompletedPath(IHttpContext context)
    {
        var options = optionsAccessor.Value;
        context.Response.Redirect(options.CompletedPath
            ?? throw new InvalidOperationException("Option 'CompletedPath' has not been provided.")
        );
    }

    private void RedirectToSignOutPath(IHttpContext context)
    {
        var options = optionsAccessor.Value;
        context.Response.Redirect(options.SignOutPath
            ?? throw new InvalidOperationException("Option 'SignOutPath' has not been provided.")
        );
    }
}
