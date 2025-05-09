using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.PublicApi.Client.Abstractions;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// Represents a service to get or set the active organisation of a user.
/// </summary>
/// <remarks>
///   <para>By default the <see cref="ActiveOrganisationClaimsProvider"/> implementation
///   of this service is used.</para>
///   <para>The default implementation can be overridden with a custom implementation
///   which persists the active organisation in a different way; for example, in a Redis
///   entry that is associated with the user session.</para>
/// </remarks>
public interface IActiveOrganisationProvider
{
    /// <summary>
    /// Sets the active organisation of a user.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="organisation">The organisation that is to become active; or, a value
    /// of <c>null</c> if no organisation is to be active.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="context"/> is null.</para>
    /// </exception>
    Task SetActiveOrganisationAsync(IHttpContext context, OrganisationDetails? organisation);

    /// <summary>
    /// Gets the active organisation state of a user.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>
    ///   <para>A value of <c>null</c> indicates that the active organisation state is
    ///   yet to be initialized for the user.</para>
    ///   <para>A <see cref="ActiveOrganisationState"/> is returned when the active
    ///   organisation state has been initialized for the user. This state represents one
    ///   of the following scenarios:</para>
    ///   <list type="bullet">
    ///     <item>No organisation is active because the user is not a member of any
    ///     organisation and is unable to make a selection.</item>
    ///     <item>No organisation is active because the application has decided to
    ///     allow the user to proceed without activating a specific organisation.</item>
    ///     <item>An organisation is active for the user.</item>
    ///   </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="context"/> is null.</para>
    /// </exception>
    Task<ActiveOrganisationState?> GetActiveOrganisationStateAsync(IHttpContext context);
}

/// <summary>
/// Represents the active organisation state of a user.
/// </summary>
public sealed record ActiveOrganisationState()
{
    /// <summary>
    /// Gets the organisation when one is active; otherwise, a value of <c>null</c> to
    /// indicate that no organisation is active for the user.
    /// </summary>
    /// <remarks>
    ///   <para>This can be a value of <c>null</c> when:</para>
    ///   <list type="bullet">
    ///     <item>No organisation is active because the user is not a member of any
    ///     organisation and is unable to make a selection.</item>
    ///     <item>No organisation is active because the application has decided to
    ///     allow the user to proceed without activating a specific organisation.</item>
    ///   </list>
    /// </remarks>
    public OrganisationDetails? Organisation { get; init; } = null;
}
