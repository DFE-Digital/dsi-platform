using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.BearerTokenAuth;

/// <summary>
/// Options for BearerTokenAuthMiddleware
/// </summary>
public sealed class BearerTokenOptions : IOptions<BearerTokenOptions>
{
    /// <summary>
    /// The ValidAudience value for validating a JWT
    /// </summary>
    public string ValidAudience { get; set; } = "signin.education.gov.uk";

    /// <summary>
    /// Number of seconds clock skew allowed when validating the JWT
    /// </summary>
    public byte ClockSkewSeconds { get; set; } = 30;

    /// <inheritdoc/>
    BearerTokenOptions IOptions<BearerTokenOptions>.Value => this;
}
