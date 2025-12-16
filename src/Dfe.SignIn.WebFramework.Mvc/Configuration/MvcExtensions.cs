using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.WebFramework.Mvc.Filters;
using Dfe.SignIn.WebFramework.Mvc.ModelBinding;

namespace Dfe.SignIn.WebFramework.Mvc.Configuration;

/// <summary>
/// MVC extensions.
/// </summary>
public static class MvcExtensions
{
    /// <summary>
    /// Registers extensions that are used throughout DfE Sign-In frontend applications.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IMvcBuilder"/> used to configure MVC services and filters.
    /// </param>
    /// <returns>
    /// The same <see cref="IMvcBuilder"/> instance, allowing fluent configuration chaining.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>Thrown if <paramref name="builder"/> is null.</para>
    /// </exception>
    public static IMvcBuilder AddDsiMvcExtensions(this IMvcBuilder builder)
    {
        ExceptionHelpers.ThrowIfArgumentNull(builder, nameof(builder));

        return builder.AddMvcOptions(options => {
            options.Filters.Add<RequestBodySizeLimitFilter>();
            options.ModelBinderProviders.Insert(0, new TrimStringModelBinderProvider());
        });
    }
}
