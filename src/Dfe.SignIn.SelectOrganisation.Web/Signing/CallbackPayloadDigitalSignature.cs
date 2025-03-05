namespace Dfe.SignIn.SelectOrganisation.Web.Signing;

/// <summary>
/// Represents the digital signature of a callback data payload.
/// </summary>
public sealed record CallbackPayloadDigitalSignature
{
    /// <summary>
    /// Gets the digital signature.
    /// </summary>
    public required string Signature { get; init; }

    /// <summary>
    /// Gets the ID of the public key which can be used to verify the digital signature.
    /// </summary>
    public required string KeyId { get; init; }
}
