using System.Security.Claims;
using Dfe.SignIn.PublicApi.Client.Abstractions;
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
/// Represents a function that can modify parameters of the user claims identity
/// when the organisation claim is being updated.
/// </summary>
/// <param name="identity">The claims identity representing the user.</param>
/// <returns>
///   <para>Typically the input <see cref="ClaimsIdentity"/> instance; however, this
///   function can opt to return a new <see cref="ClaimsIdentity"/> instance.</para>
/// </returns>
public delegate Task<ClaimsIdentity> ClaimsIdentityUpdater(ClaimsIdentity identity);

/// <summary>
/// Represents a function that can handle the sign out process.
/// </summary>
/// <param name="context">The HTTP context.</param>
public delegate Task SignOutHandler(IHttpContext context);

/// <summary>
/// A flag that indicates which role claims should be added (if any).
/// </summary>
[Flags]
public enum FetchRoleClaimsFlag
{
    /// <summary>
    /// Indicates that role claims should not be added.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Indicates that all role claims should be added.
    /// </summary>
    Everything = ~0,

    /// <summary>
    /// Indicates that the default role claim configuration should be added.
    /// </summary>
    Default = RoleName,

    /// <summary>
    /// Indicates that "roleid" claims should be added.
    /// </summary>
    RoleId = 0x01,

    /// <summary>
    /// Indicates that "rolename" claims should be added.
    /// </summary>
    RoleName = 0x02,

    /// <summary>
    /// Indicates that "rolecode" claims should be added.
    /// </summary>
    RoleCode = 0x04,

    /// <summary>
    /// Indicates that "rolenumericid" claims should be added.
    /// </summary>
    RoleNumericId = 0x08,
}

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

    /// <summary>
    /// Gets or sets whether the user roles should be fetched from the public API
    /// and then amended to the user claims each time an organisation is selected.
    /// </summary>
    /// <remarks>
    ///   <para>Specifying this will incur an additional call to the DfE Sign-in
    ///   public API to fetch the roles.</para>
    ///   <para>Only one call is made to the DfE Sign-in public API regardless of
    ///   how many role claims are desired.</para>
    /// </remarks>
    public FetchRoleClaimsFlag FetchRoleClaimsFlags { get; set; } = FetchRoleClaimsFlag.None;

    /// <summary>
    /// Gets or sets a delegate allowing an application to customise claims of the
    /// user claims identity whenever the organisation claim is updated.
    /// </summary>
    public ClaimsIdentityUpdater? UpdateClaimsIdentity { get; set; } = null;

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
    /// Gets or sets the path where the user will be redirected to sign out.
    /// </summary>
    public string SignOutPath { get; set; } = "/sign-out";

    /// <summary>
    /// Gets or sets a delegate allowing an application to customise how the
    /// sign out process is handled.
    /// </summary>
    /// <remarks>
    ///   <para>The default implementation redirects to <see cref="SignOutPath"/>.</para>
    /// </remarks>
    public SignOutHandler? OnSignOut { get; set; } = null;

    /// <inheritdoc/>
    AuthenticationOrganisationSelectorOptions IOptions<AuthenticationOrganisationSelectorOptions>.Value => this;
}
