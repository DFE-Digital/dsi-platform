using Dfe.SignIn.PublicApi.Client.Abstractions;
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
///     <item>Upon making a selection a GET request is made to a callback path within
///     the application. This callback interaction is intercepted and handled by this
///     middleware.</item>
///     <item>By default; upon selecting an organisation the standard user flow
///     (<see cref="StandardSelectOrganisationUserFlow"/>) service verifies the user
///     selection and retrieves the organisation details by making a request to the
///     DfE Sign-in API.</item>
///     <item>The selection is persisted with a <see cref="IActiveOrganisationProvider"/>
///     service which allows an application to specify this behaviour. As an example,
///     the selection could be persisted in the user session.</item>
///     <item>Finally, the middleware redirects the user to the completed path as
///     specified by <see cref="StandardSelectOrganisationUserFlowOptions.CompletedPath"/>.
///     The application must handle the completed path.</item>
///   </list>
///   <para>The various paths can be customised by modifying setting the applicable
///   options in <see cref="StandardSelectOrganisationUserFlowOptions"/>.</para>
/// </remarks>
/// <seealso cref="StandardSelectOrganisationEvents"/>
public sealed class StandardSelectOrganisationMiddleware(
    IOptions<StandardSelectOrganisationUserFlowOptions> optionsAccessor,
    ISelectOrganisationUserFlow flow,
    IActiveOrganisationProvider activeOrganisationProvider
) : IHttpMiddleware
{
    /// <inheritdoc />
    public async Task InvokeAsync(IHttpContext context, Func<Task> next)
    {
        var options = optionsAccessor.Value;

        var primaryIdentity = context.User?.GetPrimaryIdentity();
        if (context.User is null || primaryIdentity?.IsAuthenticated != true) {
            await next();
            return;
        }

        if (context.Request.Path == options.SignOutPath) {
            // Do not force user to select an organisation on this route.
            await next();
            return;
        }

        if (context.Request.Method == "GET" && context.Request.Path == options.CallbackPath) {
            await flow.ProcessCallbackAsync(context);
            return;
        }

        bool hasUserRequestedSelectOrganisation = options.EnableSelectOrganisationRequests &&
            context.Request.Method == "GET" &&
            context.Request.Path == options.SelectOrganisationRequestPath;

        var activeOrganisationState = await activeOrganisationProvider.GetActiveOrganisationStateAsync(context);

        if (hasUserRequestedSelectOrganisation || activeOrganisationState is null) {
            // Allow the user to cancel the selection flow if the selection state
            // has already been initialised previously.
            bool allowCancel = activeOrganisationState?.Organisation is not null;
            await flow.InitiateSelectionAsync(context, allowCancel);
            return;
        }

        await next();
    }
}
