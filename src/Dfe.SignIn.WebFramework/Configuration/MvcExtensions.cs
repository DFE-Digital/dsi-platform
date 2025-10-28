using Dfe.SignIn.WebFramework.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// MVC extensions.
/// </summary>
public static class MvcExtensions
{
    /// <summary>
    /// Adds automatic string trimming support to model binding.
    /// </summary>
    /// <remarks>
    ///   <para>Password data types are excluded from string trimming.</para>
    /// </remarks>
    /// <param name="options">The MVC options instance.</param>
    public static void AddTrimStringModelBinding(this MvcOptions options)
    {
        options.ModelBinderProviders.Insert(0, new TrimStringModelBinderProvider());
    }
}
