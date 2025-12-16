using Dfe.SignIn.Core.Contracts.SelectOrganisation;

namespace Dfe.SignIn.Core.Interfaces.SelectOrganisationSessions;

/// <summary>
/// Represents a repository of "select organisation" sessions.
/// </summary>
public interface ISelectOrganisationSessionRepository
{
    /// <summary>
    /// Retrieves an existing "select organisation" session.
    /// </summary>
    /// <param name="sessionKey">Unique key that identifies the session.</param>
    /// <returns>
    ///   <para>The <see cref="SelectOrganisationSessionData"/> instance when the session
    ///   exists; otherwise, a value of <c>null</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="sessionKey"/> is null or empty.</para>
    /// </exception>
    Task<SelectOrganisationSessionData?> RetrieveAsync(string sessionKey);

    /// <summary>
    /// Creates or updates a "select organisation" session.
    /// </summary>
    /// <param name="sessionKey">Unique key that identifies the session.</param>
    /// <param name="sessionData">The session data.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="sessionKey"/> is null or empty.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="sessionData"/> is null.</para>
    /// </exception>
    Task StoreAsync(string sessionKey, SelectOrganisationSessionData sessionData);

    /// <summary>
    /// Invalidates a "select organisation" session.
    /// </summary>
    /// <remarks>
    ///   <para>A session might be invalidated for a number of reasons:</para>
    ///   <list type="bullet">
    ///     <item>The user has made a selection.</item>
    ///     <item>It is nolonger necessary for the user to make a selection.</item>
    ///   </list>
    ///   <para>No exceptions are raised if the session does not exist or has
    ///   already been invalidated.</para>
    /// </remarks>
    /// <param name="sessionKey">Unique key that identifies the session.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="sessionKey"/> is null or empty.</para>
    /// </exception>
    Task InvalidateAsync(string sessionKey);
}
