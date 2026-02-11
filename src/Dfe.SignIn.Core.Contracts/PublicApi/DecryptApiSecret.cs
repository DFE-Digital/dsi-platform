using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.PublicApi;

/// <summary>
/// Request to get the decrypted API secret.
/// </summary>
[AssociatedResponse(typeof(DecryptApiSecretResponse))]
public sealed record DecryptApiSecretRequest
{
    /// <summary>
    /// The ApiSecret that needs to be decrypted.
    /// </summary>
    [Required]
    public required string EncryptedApiSecret { get; init; }
}

/// <summary>
/// Represents a response for <see cref="DecryptApiSecretRequest"/>.
/// </summary>
public sealed record DecryptApiSecretResponse
{
    /// <summary>
    /// The decrypted ApiSecret.
    /// </summary>
    public required string ApiSecret { get; init; }
}
