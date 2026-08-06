namespace Dfe.SignIn.NodeApi.Client;

/// <summary>
/// Shared constants for local development JWT authentication.
/// These values are only used when ASPNETCORE_ENVIRONMENT is "Local".
/// </summary>
public static class LocalDevAuth
{
    /// <summary>
    /// HMAC-SHA256 signing key for local JWT tokens. Safe to hardcode because
    /// the Local environment gate prevents this from ever being used in deployed environments.
    /// </summary>
    public const string SigningKey = "dsi-local-dev-signing-key-not-for-production-use!";
    
    /// <summary>
    /// Issuer for local JWT tokens.
    /// </summary>
    public const string Issuer = "local-dev";
    
    /// <summary>
    /// Audience for local JWT tokens.
    /// </summary>
    public const string Audience = "local-internal-api";
}
