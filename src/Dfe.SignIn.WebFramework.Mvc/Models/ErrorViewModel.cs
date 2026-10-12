namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// View model for a general purpose error page (version 2).
/// </summary>
public sealed record ErrorViewModel
{
    /// <summary>
    /// Gets or sets the title of the error page.
    /// </summary>
    public string PageTitle { get; set; } = "There has been an error";

    /// <summary>
    /// Gets or sets the heading of the error page.
    /// </summary>
    public string Heading { get; set; } = "There has been an error";

    /// <summary>
    /// Gets or sets the CSS class for the heading of the error page.
    /// </summary>
    public string HeadingClass { get; set; } = "govuk-heading-xl govuk-!-margin-top-3";

    /// <summary>
    /// Gets or sets the CSS class for the column of the error page.
    /// </summary>
    public string ColumnClass { get; set; } = "govuk-grid-column-two-thirds";

    /// <summary>
    /// Gets or sets the paragraphs of the error page.
    /// </summary>
    public IReadOnlyList<string> Paragraphs { get; set; } = [];

    // Navigation & Recovery

    /// <summary>
    /// Gets or sets the text for the action button on the error page.
    /// </summary>
    public string? ActionButtonText { get; set; }

    /// <summary>
    /// Gets or sets the URL for the action button on the error page.
    /// </summary>
    public string? ActionButtonUrl { get; set; }

    /// <summary>
    /// Gets or sets the back link view model for the error page.
    /// </summary>
    public BackLinkViewModel? BackLink { get; set; }

    // Support & Helpdesk

    /// <summary>
    /// Gets or sets the optional help sentence with an embedded link.
    /// Null hides the help paragraph.
    /// </summary>
    /// <remarks>
    /// Defaults match the legacy Express generic 500 page. Journey presets override this.
    /// </remarks>
    public LinkSentence? HelpLink { get; set; } = new(
        TextBefore: "If the problem continues, follow the link to ",
        LinkText: "submit a support request",
        Path: "contact-us"
    );

    // Traceability & Status Code

    /// <summary>
    /// Gets or sets a unique identifier representing the request.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the request ID should be presented.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(this.RequestId);

    /// <summary>
    /// Gets or sets the HTTP status code associated with the error.
    /// </summary>
    public int? StatusCode { get; set; }
}
