using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Dfe.SignIn.DocfxPlugin.Helpers;
using Dfe.SignIn.DocfxPlugin.Models;
using Docfx.Plugins;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Dfe.SignIn.DocfxPlugin.Processors;

/// <summary>
/// A custom post-processor plugin for DocFX which tidies up output HTML files.
/// </summary>
[Export(nameof(DsiHtmlPostProcessor), typeof(IPostProcessor))]
public sealed partial class DsiHtmlPostProcessor : IPostProcessor
{
    /// <inheritdoc/>
    public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
    {
        return metadata;
    }

    /// <inheritdoc/>
    public Manifest Process(Manifest manifest, string outputFolder, CancellationToken cancellationToken)
    {
        var tocs = TocMap.FromDirectory(outputFolder);

        var matcher = new Matcher();
        matcher.AddInclude("**.html");
        var matchingFiles = matcher.GetResultsInFullPath(outputFolder);

        Task.WaitAll([
            .. matchingFiles.Select(path => PostProcessHtmlFile(tocs, outputFolder, path, cancellationToken)),
        ], cancellationToken);

        return manifest;
    }

    [GeneratedRegex("<meta property=\"docfx:rel\" content=\"([^\"]+)\"", RegexOptions.Singleline)]
    private static partial Regex RelativePathPattern();

    private static string GetRelativePathFromMeta(string html)
    {
        var relativePathMatch = RelativePathPattern().Match(html);
        return relativePathMatch.Success
            ? relativePathMatch.Groups[1].Value
            : "/";
    }

    [GeneratedRegex("(<h1[^>]+?>)", RegexOptions.Singleline)]
    private static partial Regex MainHeadingPattern();

    [GeneratedRegex("(<pre><code class=\"lang-csharp\">)(.+?)(</pre>)", RegexOptions.Singleline)]
    private static partial Regex SampleCodePattern();

    [GeneratedRegex("<(table|thead|tr|th|tbody|td)>", RegexOptions.Singleline)]
    private static partial Regex TablePattern();

    [GeneratedRegex("^ +", RegexOptions.Multiline)]
    private static partial Regex StartLinePattern();

    [GeneratedRegex("(<h[1-6][^>]+?>)([^<]+?)(</h[1-6]>)", RegexOptions.Singleline)]
    private static partial Regex HeadingPattern();

    [GeneratedRegex("(<a[^>]+?>)([^<]+?)(</a>)", RegexOptions.Singleline)]
    private static partial Regex LinkPattern();

    [GeneratedRegex("<([ou]l)>", RegexOptions.Singleline)]
    private static partial Regex ListPattern();

    [GeneratedRegex("(<main)(.+)(</main>)", RegexOptions.Singleline)]
    private static partial Regex MainBodyPattern();

    [GeneratedRegex("[_/]", RegexOptions.Singleline)]
    private static partial Regex BreakableBeforeSymbolPattern();

    [GeneratedRegex("&(l|g)t;|[().]", RegexOptions.Singleline)]
    private static partial Regex BreakableAfterSymbolPattern();

    [GeneratedRegex("\\$\\{\\{\\s*(ENV_[^}]+)\\s*\\}\\}", RegexOptions.Singleline)]
    private static partial Regex SsiPattern();

    private static async Task PostProcessHtmlFile(
        TocMap tocs, string outputFolder, string filePath, CancellationToken cancellationToken)
    {
        var html = await File.ReadAllTextAsync(filePath, Encoding.UTF8, cancellationToken);

        string relativeRootPath = GetRelativePathFromMeta(html).TrimEnd('/');
        string pathRelativeToRoot = PathHelpers.GetRelativePath(outputFolder, filePath);
        if (!tocs.TryGetToc(pathRelativeToRoot, out var toc)) {
            return;
        }
        var tocEntry = toc!.FindEntryWithCrumbs(pathRelativeToRoot);
        bool isApiPage = toc.MemberLayout is not null;

        // Resolve parent page entry (used for page caption).
        TocEntry? parentEntry = null;
        if (isApiPage && tocEntry is not null && tocEntry.Entry.Type != "Namespace" && tocEntry.Crumbs.Length >= 3) {
            parentEntry = tocEntry.Crumbs[^3];
            if (string.IsNullOrWhiteSpace(parentEntry.Name)) {
                parentEntry = null;
            }
        }

        // Generate primary page navigation.
        if (tocs.TryGetToc(".", out var navToc)) {
            var navHtmlBuilder = new StringBuilder();
            navHtmlBuilder.Append("<ul class=\"govuk-service-navigation__list\" id=\"navigation\">");
            foreach (var item in navToc.Items ?? []) {
                if (item.Href is not null) {
                    string itemClass = "govuk-service-navigation__item";
                    if (tocEntry is not null && tocEntry.Crumbs?.Length > 0) {
                        bool isRootNavSection = tocs.TryGetToc(item.TocHref, out var navItemToc) && navItemToc == toc;
                        if (isRootNavSection || tocEntry.Crumbs.Any(crumb => crumb.Href == item.Href)) {
                            itemClass += " govuk-service-navigation__item--active";
                        }
                    }

                    string itemHref = PathHelpers.ResolveRelativePath(relativeRootPath, item.Href);

                    navHtmlBuilder.Append($"<li class=\"{itemClass}\">");
                    navHtmlBuilder.Append($"<a class=\"govuk-service-navigation__link\" href=\"{itemHref}\">{HttpUtility.HtmlEncode(item.Name)}</a>");
                    navHtmlBuilder.Append("</li>");
                }
            }
            navHtmlBuilder.Append("</ul>");
            html = html.Replace("<!--PLACEHOLDER:NAV-->", navHtmlBuilder.ToString());
        }

        // Generate breadcrumbs.
        if (tocEntry is not null) {
            var filteredCrumbs = tocEntry.Crumbs?.Where(crumb => !string.IsNullOrWhiteSpace(crumb.Href));
            if (filteredCrumbs?.Count() > 1) {
                var crumbsHtmlBuilder = new StringBuilder();
                crumbsHtmlBuilder.Append("<nav class=\"govuk-breadcrumbs\" aria-label=\"Breadcrumb\">");
                crumbsHtmlBuilder.Append("<ol class=\"govuk-breadcrumbs__list\">");
                foreach (var crumb in filteredCrumbs.SkipLast(1)) {
                    if (!string.IsNullOrWhiteSpace(crumb.Href)) {
                        string crumbHref = PathHelpers.ResolveRelativePath(relativeRootPath, crumb.Href);
                        crumbsHtmlBuilder.Append("<li class=\"govuk-breadcrumbs__list-item\">");
                        crumbsHtmlBuilder.Append($"<a class=\"govuk-breadcrumbs__link\" href=\"{crumbHref}\">{HttpUtility.HtmlEncode(crumb.Name)}</a>");
                        crumbsHtmlBuilder.Append("</li>");
                    }
                }
                crumbsHtmlBuilder.Append("</ol>");
                crumbsHtmlBuilder.Append("</nav>");
                html = html.Replace("<!--PLACEHOLDER:CRUMBS-->", crumbsHtmlBuilder.ToString());
            }
        }

        // Improve breakability of long headings.
        html = HeadingPattern().Replace(html, matches =>
            matches.Groups[1].Value +
            AddSymbolBreaks(matches.Groups[2].Value) +
            matches.Groups[3].Value
        );

        // Improve breakability of long links.
        html = LinkPattern().Replace(html, matches =>
            matches.Groups[1].Value +
            AddSymbolBreaks(matches.Groups[2].Value) +
            matches.Groups[3].Value
        );

        // Add root page title where applicable.
        if (parentEntry is not null) {
            string captionText = $"{HttpUtility.HtmlEncode(parentEntry.Type)} {HttpUtility.HtmlEncode(parentEntry.Name)}";
            captionText = AddSymbolBreaks(captionText);

            html = MainHeadingPattern().Replace(html, matches =>
                matches.Groups[1].Value + $"<span class=\"govuk-caption-l govuk-visually-hidden\">{captionText}</span>"
            );

            string pageCaption = $"<div class=\"app-page-caption\" aria-hidden=\"true\">{captionText}</div>";
            html = html.Replace("<!--PLACEHOLDER:CAPTION-->", pageCaption);
        }

        // Add side navigation.
        var sideNavRoot = (isApiPage && tocEntry is not null && tocEntry.Crumbs?.Length >= 3)
            ? tocEntry.Crumbs[^3]
            : toc;
        var sideNavigationBuilder = new StringBuilder();
        if (pathRelativeToRoot != "index.html" && BuildSideNavigationSection(sideNavigationBuilder, relativeRootPath, sideNavRoot, parentEntry, tocEntry?.Entry)) {
            html = html.Replace("<!--PLACEHOLDER:SIDENAV-->", sideNavigationBuilder.ToString());
        }
        else {
            html = html.Replace("govuk-grid-column-two-thirds", "govuk-grid-column-three-quarters");
        }

        // Use 'govuk-link' classes on all hyperlinks.
        html = MainBodyPattern().Replace(html, matches =>
            matches.Value
                .Replace("<a class=\"xref", "<a class=\"govuk-link govuk-link--no-visited-state")
                .Replace("<a href=", "<a class=\"govuk-link govuk-link--no-visited-state\" href=")
        );

        // Add GOV.UK design system classes to lists.
        html = ListPattern().Replace(html, matches => matches.Groups[1].Value switch {
            "ol" => "<ol class=\"govuk-list govuk-list--number\">",
            "ul" => "<ul class=\"govuk-list govuk-list--bullet\">",
            _ => matches.Value,
        });

        // Fix alignment of example code.
        html = SampleCodePattern().Replace(html, matches =>
            matches.Groups[1].Value +
            StartLinePattern().Replace(matches.Groups[2].Value, matches2 =>
                (matches2.Value.Length % 4 != 0)
                    ? matches2.Value[2..]
                    : matches2.Value
            ) +
            matches.Groups[3].Value
        );

        // Add GOV.UK design system classes to tables.
        html = TablePattern().Replace(html, matches => matches.Groups[1].Value switch {
            "table" => "<table class=\"govuk-table\">",
            "thead" => "<thead class=\"govuk-table__head\">",
            "tr" => "<tr class=\"govuk-table__row\">",
            "th" => "<th class=\"govuk-table__header\">",
            "tbody" => "<tbody class=\"govuk-table__body\">",
            "td" => "<td class=\"govuk-table__cell\">",
            _ => matches.Value,
        });

        // Fix SSI syntax to workaround issue where nested quote characters
        // lead to an extra space character.
        html = SsiPattern().Replace(html, "<!--#echo var=\"$1\"-->");

        await File.WriteAllTextAsync(filePath, html, Encoding.UTF8, cancellationToken);
    }

    private static string AddSymbolBreaks(string html)
    {
        return BreakableAfterSymbolPattern().Replace(
            BreakableBeforeSymbolPattern().Replace(html, "&#8203;$0"),
            "$0&#8203;"
        );
    }

    private static bool BuildSideNavigationSection(StringBuilder builder, string relativeRootPath,
        TocEntry sideNavRoot, TocEntry? parentEntry, TocEntry? activeEntry)
    {
        if (activeEntry?.Type == "Namespace") {
            return false;
        }

        var defaultSection = new TocEntry {
            Name = "Other content",
            Items = sideNavRoot.Items?.Where(item => !string.IsNullOrWhiteSpace(item.Href)).ToArray(),
        };

        builder.Append("<nav class=\"app-subnav\">");

        string? captionText = parentEntry is not null
            ? $"{HttpUtility.HtmlEncode(parentEntry.Type)} {HttpUtility.HtmlEncode(parentEntry.Name)}"
            : null;

        builder.Append($"<h2 class=\"govuk-visually-hidden\">{captionText ?? "Pages in this section"}</h2>");

        if (parentEntry is not null && !string.IsNullOrWhiteSpace(parentEntry.Href)) {
            string parentLinkClass = "govuk-button govuk-button--secondary";
            string parentHref = PathHelpers.ResolveRelativePath(relativeRootPath, parentEntry.Href);
            builder.Append($"<a class=\"{parentLinkClass}\" href=\"{parentHref}\" role=\"button\">Back to {HttpUtility.HtmlEncode(parentEntry.Type)}</a>");
        }

        var sections = (sideNavRoot.Items ?? [])
            .Append(defaultSection)
            .Where(section =>
                string.IsNullOrWhiteSpace(section.Href) &&
                section.Items is not null &&
                section.Items.Length != 0 &&
                section.Name != "Namespaces"
            );

        if (sections.Any()) {
            foreach (var section in sections) {
                builder.Append($"<h3 class=\"app-subnav__theme\">{HttpUtility.HtmlEncode(section.Name)}</h3>");
                BuildSideNavigationList(builder, relativeRootPath, section.Items!, activeEntry);
            }
        }
        else {
            if (defaultSection.Items is null || defaultSection.Items.Length == 0) {
                return false;
            }
            BuildSideNavigationList(builder, relativeRootPath, defaultSection.Items, activeEntry);
        }

        builder.Append("</nav>");

        return true;
    }

    private static void BuildSideNavigationList(StringBuilder builder, string relativeRootPath,
        IEnumerable<TocEntry> items, TocEntry? activeEntry)
    {
        builder.Append("<ul class=\"app-subnav__section\">");
        foreach (var item in items) {
            string currentClass = item.Href! == activeEntry?.Href ? " app-subnav__section-item--current" : "";
            string itemHref = PathHelpers.ResolveRelativePath(relativeRootPath, item.Href!);
            builder.Append($"<li class=\"app-subnav__section-item{currentClass}\">");
            builder.Append($"<a class=\"app-subnav__link govuk-link govuk-link--no-visited-state govuk-link--no-underline\" href=\"{itemHref}\">{HttpUtility.HtmlEncode(item.Name)}</a>");
            builder.Append("</li>");
        }
        builder.Append("</ul>");
    }
}

