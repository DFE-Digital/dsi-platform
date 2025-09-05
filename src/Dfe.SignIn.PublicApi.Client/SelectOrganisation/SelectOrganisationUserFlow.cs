using Dfe.SignIn.Core.Public.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Contracts.Organisations;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Represents a service which initiates and processes the user selection of an
/// organisation from start to finish.
/// </summary>
/// <remarks>
///   <para>Select organisation user flows can be customised by providing a custom
///   implementation of <see cref="ISelectOrganisationEvents"/>. This is
///   particuarly useful when utilising the select organisation feature for other
///   purposes.</para>
/// </remarks>
/// <seealso cref="ISelectOrganisationEvents"/>
public interface ISelectOrganisationUserFlow
{
    /// <summary>
    /// Initiates organisation selection for an authenticated user.
    /// </summary>
    /// <param name="context">The context of the current HTTP request.</param>
    /// <param name="allowCancel">Specifies whether the user can cancel selection.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="InvalidOperationException">
    ///   <para>If the user is not currently authenticated.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task InitiateSelectionAsync(IHttpContext context, bool allowCancel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify and process "select organisation" callback for an authenticated user.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other
    /// objects or threads to receive notice of cancellation.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="context"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If an unexpected callback type was encountered.</para>
    /// </exception>
    /// <exception cref="SelectOrganisationCallbackErrorException">
    ///   <para>If a callback error was encountered.</para>
    /// </exception>
    /// <exception cref="MismatchedCallbackException">
    ///   <para>If callback user ID does not match the user ID from the primary identity.</para>
    ///   <para>- or -</para>
    ///   <para>If callback is provided with an unexpected request ID.</para>
    /// </exception>
    /// <exception cref="OperationCanceledException" />
    Task ProcessCallbackAsync(IHttpContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handlers for the various events that can occur during a select organisation flow.
/// </summary>
public interface ISelectOrganisationEvents
{
    /// <summary>
    /// The event occurs when the user is to be redirected to select an organisation.
    /// </summary>
    /// <remarks>
    ///   <para>The handler should redirect the user to make a selection.</para>
    /// </remarks>
    /// <param name="context">An abstraction representing the HTTP context.</param>
    /// <param name="selectionUri">The location where the user can make a selection.</param>
    Task OnStartSelection(IHttpContext context, Uri selectionUri);

    /// <summary>
    /// The event occurs when the user cancels the organisation selection flow.
    /// </summary>
    /// <remarks>
    ///   <para>The handler should redirect the user to a suitable page.</para>
    /// </remarks>
    /// <param name="context">An abstraction representing the HTTP context.</param>
    Task OnCancelSelection(IHttpContext context);

    /// <summary>
    /// The event occurs when an error has occurred during the organisation selection flow.
    /// </summary>
    /// <param name="context">An abstraction representing the HTTP context.</param>
    /// <param name="code">The error code.</param>
    /// <exception cref="SelectOrganisationCallbackErrorException">
    ///   <para>If the handler implementation chooses to throw as exception.</para>
    /// </exception>
    /// <seealso cref="SelectOrganisationErrorCode"/>
    Task OnError(IHttpContext context, string code);

    /// <summary>
    /// The event occurs when the user has selected an organisation and the selection
    /// has been verified using the DfE Sign-in API.
    /// </summary>
    /// <remarks>
    ///   <para>The handler can choose to do something with the confirmed selection; and
    ///   then should redirect the user to a suitable page.</para>
    ///   <para><paramref name="organisation"/> is <c>null</c> when the user is to
    ///   proceed without selecting an organisation. For example, the user is unable
    ///   to select an organisation.</para>
    /// </remarks>
    /// <param name="context">An abstraction representing the HTTP context.</param>
    /// <param name="organisation">Represents the selected organisation.</param>
    Task OnConfirmSelection(IHttpContext context, OrganisationDetails? organisation);

    /// <summary>
    /// The event occurs when the user has requested to sign out.
    /// </summary>
    /// <remarks>
    ///   <para>The handler should handle the sign-out request.</para>
    /// </remarks>
    /// <param name="context">An abstraction representing the HTTP context.</param>
    Task OnSignOut(IHttpContext context);
}
