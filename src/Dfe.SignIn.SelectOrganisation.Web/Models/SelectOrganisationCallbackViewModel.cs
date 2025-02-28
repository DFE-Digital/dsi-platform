namespace Dfe.SignIn.SelectOrganisation.Web.Models;

/// <summary>
/// View model for the "Select organisation" service callback.
/// </summary>
public sealed class SelectOrganisationCallbackViewModel
{
    /// <summary>
    /// Gets the callback URL.
    /// </summary>
    public required Uri CallbackUrl { get; init; }

    /// <summary>
    /// Gets the JSON encoded payload data.
    /// </summary>
    public required string PayloadData { get; init; }

    /// <summary>
    /// Gets the digital signature of the payload data.
    /// </summary>
    public required string DigitalSignature { get; init; }

    /// <summary>
    /// Gets the key of the public key that can be used to verify the digital
    /// signature of the payload data.
    /// </summary>
    public required string PublicKeyId { get; init; }
}
