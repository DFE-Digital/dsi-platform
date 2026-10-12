namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// Predefined error models for shared HTTP status code scenarios.
/// Copy matches legacy Node.js Profile / Express helpers behaviour.
/// </summary>
public static class FrameworkErrorPresets
{
    /// <summary>
    /// Generic unhandled / server error page (Express 500 handler parity).
    /// </summary>
    /// <remarks>
    /// Callers that want a request ID shown should set <see cref="ErrorViewModel.RequestId"/>.
    /// </remarks>
    public static ErrorViewModel DefaultServerError(int statusCode = StatusCodes.Status500InternalServerError) => new() {
        PageTitle = "There has been an error",
        Heading = "There has been an error",
        Paragraphs = [],
        HelpLink = new LinkSentence(
            TextBefore: "If the problem continues, follow the link to ",
            LinkText: "submit a support request",
            Path: "contact-us"
        ),
        StatusCode = statusCode,
    };

    /// <summary>
    /// Page not found (Node <c>notFound.ejs</c> parity).
    /// </summary>
    public static ErrorViewModel NotFound() => new() {
        PageTitle = "Page not found",
        Heading = "Page not found",
        Paragraphs = [
            "If you typed the web address, check it is correct.",
            "If you pasted the web address, check you copied the entire address.",
        ],
        HelpLink = new LinkSentence(
            TextBefore: "If the web address is correct or you selected a link or button, you can ",
            LinkText: "contact the helpdesk",
            TextAfter: " to report the issue.",
            Path: "contact-us"
        ),
        StatusCode = StatusCodes.Status404NotFound,
    };

    /// <summary>
    /// Not authorised (Node <c>notAuthorised.ejs</c> parity).
    /// </summary>
    public static ErrorViewModel NotAuthorised(int statusCode = StatusCodes.Status403Forbidden) => new() {
        PageTitle = "You are not authorised to view this information",
        Heading = "You are not authorised to view this information",
        HeadingClass = "govuk-heading-m govuk-!-margin-top-8",
        Paragraphs = [],
        HelpLink = null,
        StatusCode = statusCode,
    };
}
