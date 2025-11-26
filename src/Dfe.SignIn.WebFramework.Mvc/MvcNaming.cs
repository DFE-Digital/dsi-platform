using System.Text.RegularExpressions;

namespace Dfe.SignIn.WebFramework.Mvc;

/// <summary>
/// Naming helpers for MVC applications.
/// </summary>
public static partial class MvcNaming
{
    /// <summary>
    /// Retrieves the friendly name of a controller.
    /// </summary>
    /// <remarks>
    ///   <para>Example of usage within a controller action:</para>
    ///   <code language="csharp"><![CDATA[
    ///     return this.RedirectToAction(
    ///         nameof(ContactController.PostIndex),
    ///         MvcNaming.Controller<ContactController>()
    ///     );
    ///   ]]></code>
    ///   <para>Example of usage with a Razor view hyperlink:</para>
    ///   <code language="razor"><![CDATA[
    ///     <a asp-action=@nameof(ContactController.Index)
    ///        asp-controller=@(MvcNaming.Controller<ContactController>())
    ///     >
    ///         Click here to contact us
    ///     </a>
    ///   ]]></code>
    ///   <para>Example of usage with a Razor view form:</para>
    ///   <code language="razor"><![CDATA[
    ///     <form method="post" asp-antiforgery=true>
    ///         <govuk-button
    ///             asp-action=@nameof(ContactController.PostIndex)
    ///             asp-controller=@(MvcNaming.Controller<ContactController>())
    ///         >
    ///             Submit
    ///         </govuk-button>
    ///     </form>
    ///   ]]></code>
    /// </remarks>
    /// <typeparam name="TController">The type of controller.</typeparam>
    /// <returns>
    ///   <para>Friendly name of the controller; for example, "Home" from "HomeController".</para>
    /// </returns>
    public static string Controller<TController>()
        => ControllerSuffixRegex().Replace(typeof(TController).Name, "");

    [GeneratedRegex("Controller$")]
    private static partial Regex ControllerSuffixRegex();
}
