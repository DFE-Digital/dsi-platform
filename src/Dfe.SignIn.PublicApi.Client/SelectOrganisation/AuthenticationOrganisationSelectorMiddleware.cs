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
    IOptions<AuthenticationOrganisationSelectorOptions> optionsAccessor,
    IAuthenticationOrganisationSelector organisationSelector,
    ISelectOrganisationCallbackProcessor callbackProcessor,
    IOrganisationClaimManager organisationClaimManager,
    RequestDelegate next)
{
    /// <summary>
    /// Called when "select organisation" authentication middleware is being used.
    /// </summary>
    /// <param name="context">The context of the current HTTP request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity!.IsAuthenticated) {
            var options = optionsAccessor.Value;

            if (context.Request.Method == HttpMethods.Post && context.Request.Path == options.SelectOrganisationCallbackPath) {
                string callbackDataJson = await callbackProcessor.ProcessCallbackJsonAsync(context.Request);
                await organisationClaimManager.UpdateOrganisationClaimAsync(context, callbackDataJson);
                context.Response.Redirect(options.CompletedPath);
                return;
            }

            bool userRequestedSelectOrganisation = options.EnableSelectOrganisationRequests &&
                context.Request.Method == HttpMethods.Get &&
                context.Request.Path == options.SelectOrganisationRequestPath;

            bool hasCheckedSelectOrganisationRequirement = context.User.HasClaim(
                claim => claim.Type == DsiClaimTypes.Organisation
            );

            if (userRequestedSelectOrganisation || !hasCheckedSelectOrganisationRequirement) {
                await organisationSelector.InitiateSelectionAsync(context);
                return;
            }
        }

        await next(context);
    }
}
