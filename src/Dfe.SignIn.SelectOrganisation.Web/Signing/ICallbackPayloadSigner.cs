namespace Dfe.SignIn.SelectOrganisation.Web.Signing;

/// <summary>
/// Represents a service that can create a digital signature from callback payload data.
/// </summary>
public interface ICallbackPayloadSigner
{
    /// <summary>
    /// Create digital signature from the given payload data.
    /// </summary>
    /// <param name="data">Payload data.</param>
    /// <returns>
    ///   <para>The digital signature.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="data"/> is null.</para>
    /// </exception>
    CallbackPayloadDigitalSignature CreateDigitalSignature(string data);
}
