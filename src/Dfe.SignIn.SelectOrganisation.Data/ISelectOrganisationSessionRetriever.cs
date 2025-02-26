namespace Dfe.SignIn.SelectOrganisation.Data;

/// <summary>
/// Represents a service that retrieves "select organisation" sessions.
/// </summary>
public interface ISelectOrganisationSessionRetriever
{
    /// <summary>
    /// Retrieves an existing "select organisation" session.
    /// </summary>
    /// <param name="sessionKey">Unique key that identifies the session.</param>
    /// <returns>
    ///   <para>The <see cref="SelectOrganisationSessionData"/> instance when the session
    ///   exists; otherwise, a value of <c>null</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="sessionKey"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="sessionKey"/> is an empty string.</para>
    /// </exception>
    Task<SelectOrganisationSessionData?> RetrieveSession(string sessionKey);
}
