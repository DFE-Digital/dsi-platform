using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Represents a function that can modify the parameters of a "select organisation"
/// request during the user authentication journey.
/// </summary>
/// <param name="request">The request to create a "select organisation" session.</param>
/// <returns>
///   <para>The modified "select organisation" request.</para>
/// </returns>
public delegate CreateSelectOrganisationSession_PublicApiRequest SelectOrganisationRequestPreparer(
    CreateSelectOrganisationSession_PublicApiRequest request
);

/// <summary>
/// Options for when an organisation is being selected when a user is authenticating.
/// </summary>
/// <remarks>
///   <para>These options do not apply when using the "select organisation" feature
///   of DfE Sign-in for alternative purposes.</para>
/// </remarks>
public sealed class AuthenticationOrganisationSelectorOptions : IOptions<AuthenticationOrganisationSelectorOptions>
{
    /// <summary>
    /// Gets or sets whether the user can switch organisation once they have already
    /// selected an organisation.
    /// </summary>
    /// <remarks>
    ///   <para>A user can make a GET request on the path <see cref="SelectOrganisationRequestPath"/>
    ///   to re-trigger organisation selection. The user claims are updated when the
    ///   organisation selection changes.</para>
    /// </remarks>
    /// <seealso cref="SelectOrganisationRequestPath"/>
    public bool EnableSelectOrganisationRequests { get; set; } = false;

    /// <summary>
    /// Gets or sets the request path which the user can use to select a different
    /// organisation having already selected an organisation.
    /// </summary>
    /// <remarks>
    ///   <para>This is only applicable when <see cref="EnableSelectOrganisationRequests"/>
    ///   is configured as <c>true</c>.</para>
    ///   <para>This path is handled automatically by the <see cref="AuthenticationOrganisationSelectorMiddleware"/>
    ///   middleware; no custom logic is required for this route.</para>
    /// </remarks>
    /// <seealso cref="EnableSelectOrganisationRequests"/>
    public string SelectOrganisationRequestPath { get; set; } = "/select-organisation";

    /// <summary>
    /// Gets or sets the path where the DfE Sign-in "select organisation" frontend
    /// will callback to.
    /// </summary>
    /// <remarks>
    ///   <para>This path is handled automatically by the <see cref="AuthenticationOrganisationSelectorMiddleware"/>
    ///   middleware; no custom logic is required for this route.</para>
    /// </remarks>
    public string SelectOrganisationCallbackPath { get; set; } = "/callback/select-organisation";

    /// <summary>
    /// Gets or sets the path where the user will be redirected after they have
    /// completed selecting an organisation.
    /// </summary>
    /// <remarks>
    ///   <para>By default this redirects to the root page of the application.</para>
    ///   <para>The application must handle requests to this path.</para>
    /// </remarks>
    public string CompletedPath { get; set; } = "/";

    /// <summary>
    /// Gets or sets a delegate allowing an application to customise the parameters of
    /// the "select organisation" request during the authentication process.
    /// </summary>
    /// <remarks>
    ///   <para>This only applies to "select organisation" requests that are initiated
    ///   by the <see cref="AuthenticationOrganisationSelectorMiddleware"/>. If the
    ///   application is making use of the DfE Sign-in "select organisation" feature
    ///   for other purposes; then those requests can be configured directly.</para>
    /// </remarks>
    public SelectOrganisationRequestPreparer? PrepareSelectOrganisationRequest { get; set; } = null;

    /// <inheritdoc/>
    AuthenticationOrganisationSelectorOptions IOptions<AuthenticationOrganisationSelectorOptions>.Value => this;
}
