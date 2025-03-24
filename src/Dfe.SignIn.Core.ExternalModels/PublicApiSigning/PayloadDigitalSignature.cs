namespace Dfe.SignIn.Core.ExternalModels.PublicApiSigning;

/// <summary>
/// Represents the digital signature of a data payload.
/// </summary>
public sealed record PayloadDigitalSignature()
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
