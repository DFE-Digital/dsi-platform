
namespace Dfe.SignIn.Core.Contracts.PublicApi;

/// <summary>
/// Request to get an ApiSecret encrypted.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="EncryptPublicApiSecretResponse"/></item>
///   </list>
/// </remarks>
public sealed record EncryptPublicApiSecretRequest
{
    /// <summary>
    /// The ApiSecret that needs to be encrypted.
    /// </summary>
    public required string ApiSecret { get; init; }
}

/// <summary>
/// Represents a response for <see cref="EncryptPublicApiSecretRequest"/>.
/// </summary>
public sealed record EncryptPublicApiSecretResponse
{
    /// <summary>
    /// The encrypted ApiSecret.
    /// </summary>
    public required string EncryptedApiSecret { get; init; }
}
