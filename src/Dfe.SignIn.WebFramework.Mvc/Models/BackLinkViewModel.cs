namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// The view model for a back link.
/// </summary>
public sealed class BackLinkViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackLinkViewModel"/> class.
    /// </summary>
    public BackLinkViewModel() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BackLinkViewModel"/> class from relative URL.
    /// </summary>
    /// <param name="relativeHref">The relative link.</param>
    public BackLinkViewModel(string? relativeHref)
    {
        this.Href = new Uri(relativeHref ?? "/", UriKind.Relative);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BackLinkViewModel"/> class from a URI.
    /// </summary>
    /// <param name="href">The URI.</param>
    public BackLinkViewModel(Uri href)
    {
        this.Href = href;
    }

    /// <summary>
    /// The text to show on the back link (defaults to "Back").
    /// </summary>
    public string Text { get; set; } = "Back";

    /// <summary>
    /// The hypertext reference.
    /// </summary>
    public Uri Href { get; set; } = new Uri("/", UriKind.Relative);
}
