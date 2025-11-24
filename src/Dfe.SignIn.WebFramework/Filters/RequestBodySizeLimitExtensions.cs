using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Base.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.WebFramework.Filters;

/// <summary>
/// Extension method for <see cref="RequestBodySizeLimitFilter"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public static class RequestBodySizeLimitExtensions
{
    /// <summary>
    /// Registers the <see cref="RequestBodySizeLimitFilter"/> as a global MVC filter.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IMvcBuilder"/> used to configure MVC services and filters.
    /// </param>
    /// <returns>
    /// The same <see cref="IMvcBuilder"/> instance, allowing fluent configuration chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>Thrown if <paramref name="builder"/> is null.</para>
    /// </exception>
    public static IMvcBuilder AddRequestBodySizeLimitFilter(this IMvcBuilder builder)
    {
        ExceptionHelpers.ThrowIfArgumentNull(builder, nameof(builder));

        builder.Services.Configure<MvcOptions>(options => {
            options.Filters.Add<RequestBodySizeLimitFilter>();
        });

        return builder;
    }
}
