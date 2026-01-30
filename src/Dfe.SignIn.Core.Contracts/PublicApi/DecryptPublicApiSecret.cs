namespace Dfe.SignIn.Core.Contracts.PublicApi;

/// <summary>
/// Request to get the decrypted API secret.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="DecryptedPublicApiSecretResponse"/></item>
///   </list>
/// </remarks>
public sealed record DecryptPublicApiSecretRequest
{
    /// <summary>
    /// The ApiSecret that needs to be decrypted.
    /// </summary>
    public required string EncryptedApiSecret { get; init; }
}

/// <summary>
/// Represents a response for <see cref="DecryptPublicApiSecretRequest"/>.
/// </summary>
public sealed record DecryptedPublicApiSecretResponse
{
    /// <summary>
    /// The decrypted ApiSecret.
    /// </summary>
    public required string ApiSecret { get; init; }
}
