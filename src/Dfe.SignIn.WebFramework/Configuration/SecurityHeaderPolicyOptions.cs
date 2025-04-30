using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Security header policy options.
/// </summary>
public sealed class SecurityHeaderPolicyOptions : IOptions<SecurityHeaderPolicyOptions>
{
    /// <summary>
    /// The default "Allowed Origin" for the DfE Sign-in platform covering all environments.
    /// </summary>
    public const string DefaultAllowedOrigin = "*.signin.education.gov.uk";

    /// <summary>
    /// Gets a value indicating if the <c>Strict-Transport-Security</c> should be disabled.
    /// </summary>
    /// <remarks>
    ///   <para>This defaults to a value of <c>false</c>.</para>
    ///   <para>Whilst this is sometimes useful when testing locally; this option should not
    ///   be set to <c>true</c> in a hosted environment.</para>
    /// </remarks>
    public bool DisableStrictTransportSecurityHeader { get; set; } = false;

    /// <summary>
    /// Gets the HTTP Strict Transport Security maximum age in seconds.
    /// </summary>
    /// <remarks>
    ///   <para>This defaults to 365 days.</para>
    /// </remarks>
    public int HstsMaxAgeInSeconds { get; set; } = (int)TimeSpan.FromDays(365).TotalSeconds;

    /// <summary>
    /// Gets the list of allowed origins.
    /// </summary>
    /// <remarks>
    ///   <para>Defaults to allow all sub-domains of the DfE Sign-in platform covering
    ///   all environments (see <see cref="DefaultAllowedOrigin"/>).</para>
    /// </remarks>
    public string[] AllowedOrigins { get; set; } = [DefaultAllowedOrigin];

    /// <inheritdoc />
    SecurityHeaderPolicyOptions IOptions<SecurityHeaderPolicyOptions>.Value => this;
}
