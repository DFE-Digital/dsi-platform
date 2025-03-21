using Dfe.SignIn.Core.PublicModels.PublicApiSigning;

namespace Dfe.SignIn.PublicApiClient.PublicApiSigning;

/// <summary>
/// Represents a service that verifies the integrity of a payload data by verifying
/// its digital signature.
/// </summary>
public interface IPayloadVerifier
{
    /// <summary>
    /// Verifies the digital signature of a payload data.
    /// </summary>
    /// <param name="data">The payload data.</param>
    /// <param name="signature">The digital signature.</param>
    /// <returns>
    ///   <para>A value of true if the digital signature was verified to be valid;
    ///   otherwise, false.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="data"/> is null</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="signature"/> is null</para>
    /// </exception>
    Task<bool> VerifyPayload(string data, PayloadDigitalSignature signature);
}
