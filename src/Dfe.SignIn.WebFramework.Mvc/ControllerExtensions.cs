using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.WebFramework.Mvc;

/// <summary>
/// Controller extension methods.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Creates an error view and includes the unique request ID to improve traceability.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="viewName">Optional, name of the view.</param>
    /// <param name="model">Optional, customised model for error view.</param>
    /// <returns>
    ///   <para>The action result for the view.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="controller"/> is null.</para>
    /// </exception>
    public static IActionResult ErrorView(this Controller controller, string? viewName = null, ErrorViewModel? model = null)
    {
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));

        model ??= new ErrorViewModel();
        model.RequestId = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier;

        return controller.View(viewName, model);
    }
}
