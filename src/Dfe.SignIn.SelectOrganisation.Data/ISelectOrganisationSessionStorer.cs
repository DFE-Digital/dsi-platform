namespace Dfe.SignIn.SelectOrganisation.Data;

/// <summary>
/// Represents a service that stores and invalidates "select organisation" sessions.
/// </summary>
public interface ISelectOrganisationSessionStorer
{
    /// <summary>
    /// Creates or updates a "select organisation" session.
    /// </summary>
    /// <param name="sessionKey">Unique key that identifies the session.</param>
    /// <param name="sessionData">The session data.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="sessionKey"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="sessionData"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="sessionKey"/> is an empty string.</para>
    /// </exception>
    Task StoreSessionAsync(string sessionKey, SelectOrganisationSessionData sessionData);

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
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="sessionKey"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="sessionKey"/> is an empty string.</para>
    /// </exception>
    Task InvalidateSessionAsync(string sessionKey);
}
