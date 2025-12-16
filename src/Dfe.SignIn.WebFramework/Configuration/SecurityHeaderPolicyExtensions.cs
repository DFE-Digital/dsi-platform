using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Extension methods for setting up security header policy.
/// </summary>
[ExcludeFromCodeCoverage]
public static class SecurityHeaderPolicyExtensions
{
    /// <summary>
    /// Use the standard security header policy for the DfE Sign-in platform.
    /// </summary>
    /// <param name="builder">The builder to register the middleware on.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="builder"/> is null.</para>
    /// </exception>
    public static void UseDsiSecurityHeaderPolicy(this IApplicationBuilder builder)
    {
        ExceptionHelpers.ThrowIfArgumentNull(builder, nameof(builder));

        var optionsAccessor = builder.ApplicationServices.GetRequiredService<
            IOptions<SecurityHeaderPolicyOptions>
        >();
        builder.UseSecurityHeaders(GetHeaderPolicyCollection(optionsAccessor.Value));
    }

    private static HeaderPolicyCollection GetHeaderPolicyCollection(SecurityHeaderPolicyOptions options)
    {
        var headerPolicy = new HeaderPolicyCollection();

        if (!options.DisableStrictTransportSecurityHeader) {
            headerPolicy.AddStrictTransportSecurityMaxAgeIncludeSubDomains(options.HstsMaxAgeInSeconds);
        }

        headerPolicy
            .AddCustomHeader("X-Download-Options", "noopen")
            .AddFrameOptionsDeny()
            .AddXssProtectionDisabled()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyNoReferrer()
            .RemoveServerHeader()
            .AddCrossOriginOpenerPolicy(builder => {
                builder.SameOrigin();
            })
            .AddCrossOriginEmbedderPolicy(builder => {
                builder.RequireCorp();
            })
            .AddCrossOriginResourcePolicy(builder => {
                builder.SameOrigin();
            })
            .AddContentSecurityPolicy(builder => {
                builder.AddDefaultSrc().Self().From(options.AllowedOrigins);
                builder.AddScriptSrc().Self().From(options.AllowedOrigins).WithNonce();
            })
            .AddPermissionsPolicyWithDefaultSecureDirectives();

        return headerPolicy;
    }
}
