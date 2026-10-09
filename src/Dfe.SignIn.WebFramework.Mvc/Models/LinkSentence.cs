namespace Dfe.SignIn.WebFramework.Mvc.Models;

/// <summary>
/// A sentence that embeds a hyperlink between leading and trailing text.
/// </summary>
/// <param name="TextBefore">Text preceding the link.</param>
/// <param name="LinkText">Anchor text.</param>
/// <param name="TextAfter">Text following the link.</param>
/// <param name="Path">
/// Relative path resolved against a base URL when <paramref name="Href"/> is not set
/// (e.g. <c>contact-us</c>).
/// </param>
/// <param name="Href">
/// Fully resolved URL. When set, this takes precedence over <paramref name="Path"/>.
/// </param>
public sealed record LinkSentence(
    string TextBefore,
    string LinkText,
    string TextAfter = ".",
    string? Path = null,
    string? Href = null
)
{
    /// <summary>
    /// Resolves the link target from <see cref="Href"/> or <see cref="Path"/>.
    /// </summary>
    /// <param name="baseUrl">Base URL used when resolving <see cref="Path"/>.</param>
    /// <param name="query">Optional query string without a leading <c>?</c>.</param>
    /// <returns>The resolved href.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when neither <see cref="Href"/> nor <see cref="Path"/> is provided,
    /// or when <see cref="Path"/> is used without a <paramref name="baseUrl"/>.
    /// </exception>
    public string ResolveHref(Uri? baseUrl = null, string? query = null)
    {
        string href;
        if (!string.IsNullOrEmpty(this.Href)) {
            href = this.Href;
        }
        else if (!string.IsNullOrEmpty(this.Path)) {
            if (baseUrl is null) {
                throw new InvalidOperationException(
                    $"{nameof(LinkSentence)} with {nameof(this.Path)} requires a base URL.");
            }

            href = new Uri(baseUrl, this.Path).ToString();
        }
        else {
            throw new InvalidOperationException(
                $"{nameof(LinkSentence)} requires either {nameof(this.Href)} or {nameof(this.Path)}.");
        }

        if (string.IsNullOrEmpty(query)) {
            return href;
        }

        var separator = href.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{href}{separator}{query}";
    }
}
