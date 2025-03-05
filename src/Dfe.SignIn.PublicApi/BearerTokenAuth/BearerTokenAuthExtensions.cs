namespace Dfe.SignIn.PublicApi.BearerTokenAuth;

/// <summary>
/// Extension method for setting up BearerTokenAuthMiddleware
/// </summary>
public static class BearerTokenAuthExtensions
{
    /// <summary>
    /// Setup BearerTokenAuthMiddleware
    /// </summary>
    /// <param name="builder">The builder to register the middleware on.</param>
    /// <param name="setupAction">An actions to configure the provided options.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="builder"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="setupAction"/> is null.</para>
    /// </exception>
    public static IApplicationBuilder UseBearerTokenAuthMiddleware(this IApplicationBuilder builder, Action<BearerTokenOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        var options = new BearerTokenOptions();
        setupAction.Invoke(options);

        return builder.UseMiddleware<BearerTokenAuthMiddleware>(options);
    }
}