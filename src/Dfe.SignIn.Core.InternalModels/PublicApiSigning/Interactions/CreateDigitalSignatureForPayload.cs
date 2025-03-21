using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;

namespace Dfe.SignIn.Core.InternalModels.PublicApiSigning.Interactions;

/// <summary>
/// Represents a request to create a digital signature for a data payload.
/// </summary>
public sealed record CreateDigitalSignatureForPayloadRequest()
{
    /// <summary>
    /// Gets the payload data.
    /// </summary>
    public required string Payload { get; init; }
}

/// <summary>
/// Represents a response for <see cref="CreateDigitalSignatureForPayloadRequest"/>.
/// </summary>
public sealed record CreateDigitalSignatureForPayloadResponse()
{
    /// <summary>
    /// Gets the digital signature.
    /// </summary>
    public required PayloadDigitalSignature Signature { get; init; }
}
