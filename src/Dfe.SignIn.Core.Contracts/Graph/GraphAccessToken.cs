namespace Dfe.SignIn.Core.Contracts.Graph;

/// <summary>
/// Represents an access token with expiry information for use with Graph API.
/// </summary>
public sealed record GraphAccessToken
{
    /// <summary>
    /// The access token value.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The time when the provided token expires.
    /// </summary>
    public required DateTimeOffset ExpiresOn { get; init; }
}
