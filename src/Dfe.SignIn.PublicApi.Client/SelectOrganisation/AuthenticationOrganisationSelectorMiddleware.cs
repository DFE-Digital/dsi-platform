using System.Text.Json;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// This middleware routes authenticated users through to the "select organisation"
/// feature of DfE Sign-in so that they can choose which organisation to authenticate.
/// </summary>
/// <remarks>
///   <para>The user is not prompted to select an organisation if they are not
///   authenticated. This means that the user can interact with application endpoints
///   that do not require authentication.</para>
///   <para>If a user is authenticated but is yet to select an organisation; then they
///   can be prompted to select an organisation:</para>
///   <list type="number">
///     <item>A "select organisation" session is created in DfE Sign-in using the
///     applicable public API endpoint.</item>
///     <item>The user is redirected to the "select organisation" DfE Sign-in frontend
///     where they are prompted to make a selection.</item>
///     <item>Upon making a selection a POST request is made to a callback path within
///     the application. This callback interaction is intercepted and handled by this
///     middleware.</item>
///     <item>The middleware verifies that the callback data has not been tampered
///     with by verifying its digital signature against the corresponding public key
///     provided from the DfE Sign-in Public API.</item>
///     <item>The middleware then amends the authenticated user identity with the
///     additional organisation claim.</item>
///     <item>Finally, the middleware redirects the user to the completed path as
///     specified by <see cref="AuthenticationOrganisationSelectorOptions.CompletedPath"/>.
///     The application must handle the completed path.</item>
///   </list>
///   <para>The various paths can be customised by modifying setting the applicable
///   options in <see cref="AuthenticationOrganisationSelectorOptions"/>.</para>
/// </remarks>
public sealed class AuthenticationOrganisationSelectorMiddleware(
    IJsonSerializerOptionsAccessor jsonSerializerOptionsAccessor,
    IOptions<AuthenticationOrganisationSelectorOptions> optionsAccessor,
    IAuthenticationOrganisationSelector organisationSelector,
    ISelectOrganisationCallbackProcessor callbackProcessor,
    IOrganisationClaimManager organisationClaimManager,
    RequestDelegate next)
{
    // Workaround .NET Core bug where [FromKeyedService] does not work in middleware.
    // See: https://github.com/dotnet/aspnetcore/issues/54500
    private JsonSerializerOptions JsonOptions => jsonSerializerOptionsAccessor.GetOptions();

    /// <summary>
    /// Called when "select organisation" authentication middleware is being used.
    /// </summary>
    /// <param name="context">The context of the current HTTP request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity!.IsAuthenticated) {
            var options = optionsAccessor.Value;

            if (context.Request.Path == options.SignOutPath) {
                // Do not force user to select an organisation on this route.
                await next(context);
                return;
            }

            if (context.Request.Method == HttpMethods.Post && context.Request.Path == options.SelectOrganisationCallbackPath) {
                await this.HandleSelectOrganisationCallbackAsync(context, options);
                return;
            }

            bool userRequestedSelectOrganisation = options.EnableSelectOrganisationRequests &&
                context.Request.Method == HttpMethods.Get &&
                context.Request.Path == options.SelectOrganisationRequestPath;

            bool hasCheckedSelectOrganisationRequirement = context.User.HasClaim(
                claim => claim.Type == DsiClaimTypes.Organisation
            );

            if (userRequestedSelectOrganisation || !hasCheckedSelectOrganisationRequirement) {
                await organisationSelector.InitiateSelectionAsync(context, cancellationToken: default);
                return;
            }
        }

        await next(context);
    }

    private async Task HandleSelectOrganisationCallbackAsync(
        HttpContext context,
        AuthenticationOrganisationSelectorOptions options)
    {
        var callbackViewModel = SelectOrganisationCallbackViewModel.FromRequest(context.Request);
        var callbackData = await callbackProcessor.ProcessCallbackAsync(callbackViewModel);

        if (callbackData is SelectOrganisationCallbackId) {
            // An organisation was selected.
            string callbackDataJson = JsonSerializer.Serialize(callbackData, callbackData.GetType(), this.JsonOptions);
            await organisationClaimManager.UpdateOrganisationClaimAsync(context, callbackDataJson);
        }
        else if (callbackData is SelectOrganisationCallbackCancel) {
            // Selection was cancelled.
            if (!context.User.HasClaim(claim => claim.Type == DsiClaimTypes.Organisation)) {
                // User has not selected an organisation yet; sign them out.
                await this.HandleSignOut(context, options);
                return;
            }
        }
        else if (callbackData is SelectOrganisationCallbackSignOut) {
            // User has requested to sign-out.
            await this.HandleSignOut(context, options);
            return;
        }
        else {
            throw new InvalidOperationException($"Encountered unexpected callback payload type '{callbackViewModel.PayloadType}'.");
        }

        context.Response.Redirect(options.CompletedPath);
    }

    private Task HandleSignOut(
        HttpContext context,
        AuthenticationOrganisationSelectorOptions options)
    {
        if (options.HandleSignOut is not null) {
            return options.HandleSignOut(context);
        }

        if (options.SignOutPath is null) {
            throw new InvalidOperationException("Option 'SignOutPath' has not been provided.");
        }

        context.Response.Redirect(options.SignOutPath);

        return Task.CompletedTask;
    }
}
