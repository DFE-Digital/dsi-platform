using System.Text.RegularExpressions;

namespace Dfe.SignIn.WebFramework;

/// <summary>
/// Naming helpers for MVC applications.
/// </summary>
public static partial class MvcNaming
{
    /// <summary>
    /// Retrieves the friendly name of a controller.
    /// </summary>
    /// <typeparam name="TController">The type of controller.</typeparam>
    /// <returns>
    ///   <para>Friendly name of the controller; for example, "Home" from "HomeController".</para>
    /// </returns>
    public static string Controller<TController>()
        => ControllerSuffixRegex().Replace(typeof(TController).Name, "");

    [GeneratedRegex("Controller$")]
    private static partial Regex ControllerSuffixRegex();
}
