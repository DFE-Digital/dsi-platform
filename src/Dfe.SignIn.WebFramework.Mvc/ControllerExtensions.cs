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
    /// The default error view name.
    /// </summary>
    public static readonly string DefaultErrorViewName = "/Views/Error/Index.cshtml";

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
    public static IActionResult ErrorView(this Controller controller, string? viewName = null, LegacyErrorViewModel? model = null)
    {
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));

        model ??= new LegacyErrorViewModel();
        model.RequestId = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier;

        return controller.View(viewName, model);
    }

    /// <summary>
    /// Creates an error view and includes the unique request ID to improve traceability.
    /// </summary>
    /// <param name="controller">The controller.</param>
    /// <param name="model">The error view model.</param>
    /// <param name="viewName">The name of the view.</param>
    /// <returns>The action result for the view.</returns>
    public static IActionResult ErrorView(this Controller controller, ErrorViewModel model, string? viewName = null)
    {
        ExceptionHelpers.ThrowIfArgumentNull(controller, nameof(controller));
        ExceptionHelpers.ThrowIfArgumentNull(model, nameof(model));

        viewName ??= DefaultErrorViewName;

        if (model.StatusCode.HasValue) {
            controller.Response.StatusCode = model.StatusCode.Value;
        }

        return controller.View(viewName, model);
    }
}
