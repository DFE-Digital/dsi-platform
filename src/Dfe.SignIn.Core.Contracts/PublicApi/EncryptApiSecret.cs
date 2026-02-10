
using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Contracts.PublicApi;

/// <summary>
/// Request to get an ApiSecret encrypted.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="EncryptApiSecretResponse"/></item>
///   </list>
/// </remarks>
public sealed record EncryptApiSecretRequest
{
    /// <summary>
    /// The ApiSecret that needs to be encrypted.
    /// </summary>
    [Required]
    public required string ApiSecret { get; init; }
}

/// <summary>
/// Represents a response for <see cref="EncryptApiSecretRequest"/>.
/// </summary>
public sealed record EncryptApiSecretResponse
{
    /// <summary>
    /// The encrypted ApiSecret.
    /// </summary>
    public required string EncryptedApiSecret { get; init; }
}
