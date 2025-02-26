namespace Dfe.SignIn.SelectOrganisation.Data.Json;

/// <summary>
/// Represents a service that serializes and deserializes <see cref="SelectOrganisationSessionData"/>.
/// </summary>
public interface ISessionDataSerializer
{
    /// <summary>
    /// Serialize session data to a JSON encoded string.
    /// </summary>
    /// <param name="sessionData">A session data object.</param>
    /// <returns>
    ///   <para>The JSON encoded representation of the session data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="sessionData"/> is null.</para>
    /// </exception>
    string Serialize(SelectOrganisationSessionData sessionData);

    /// <summary>
    /// Deserialize session data from a JSON encoded string.
    /// </summary>
    /// <param name="sessionDataJson">A JSON encoded representation of a session data.</param>
    /// <returns>
    ///   <para>The JSON encoded representation of the session data.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="sessionDataJson"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="sessionDataJson"/> is an empty string.</para>
    /// </exception>
    SelectOrganisationSessionData Deserialize(string sessionDataJson);
}
