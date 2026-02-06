using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.PublicApi;

/// <summary>
/// Request to get the decrypted API secret.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="DecryptApiSecretResponse"/></item>
///   </list>
/// </remarks>
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
