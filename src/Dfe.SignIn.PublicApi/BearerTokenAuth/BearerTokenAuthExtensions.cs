using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.PublicApi.BearerTokenAuth;

/// <summary>
/// Extension method for setting up BearerTokenAuthMiddleware
/// </summary>
public static class BearerTokenAuthExtensions
{
    /// <summary>
    /// Register bearer token auth middleware into application.
    /// </summary>
    /// <param name="builder">The builder to register the middleware on.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="builder"/> is null.</para>
    /// </exception>
    public static void UseBearerTokenAuthMiddleware(this IApplicationBuilder builder)
    {
        ExceptionHelpers.ThrowIfArgumentNull(builder, nameof(builder));

        builder.UseMiddleware<BearerTokenAuthMiddleware>();
    }
}
