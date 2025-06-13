using System.Globalization;
using System.Text.RegularExpressions;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.Web.Help.Content;

/// <summary>
/// Helper functionality for working with topic paths and slugs.
/// </summary>
public static partial class TopicPathHelpers
{
    /// <summary>
    /// The default topic title to fallback to when an empty slug is provided.
    /// </summary>
    public const string DefaultTitle = "Untitled";

    /// <summary>
    /// Transforms a topic path to a title.
    /// </summary>
    /// <param name="topicPath">The topic path; for example, "account/multifactor-authentication".</param>
    /// <returns>
    ///   <para>The transformed title; for example, "Multifactor authentication".</para>
    ///   <para>A default title when <paramref name="topicPath"/> is an empty string.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    /// </exception>
    public static string TopicPathToTitle(string topicPath)
    {
        string? slug = SlugFromPath(topicPath);

        if (string.IsNullOrWhiteSpace(slug)) {
            return DefaultTitle;
        }

        string[] words = slug.Split('-');
        words[0] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[0]);
        return string.Join(' ', words);
    }

    [GeneratedRegex(@"[^/]+$")]
    private static partial Regex ExtractSlugPattern();

    /// <summary>
    /// Extract slug from the given topic path.
    /// </summary>
    /// <param name="topicPath">The topic path.</param>
    /// <returns>
    ///   <para>The slug when one is present; otherwise, a value of null.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    /// </exception>
    public static string? SlugFromPath(string topicPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicPath, nameof(topicPath));

        var match = ExtractSlugPattern().Match(topicPath.TrimEnd('/'));
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"^/[^/]+$")]
    private static partial Regex IsParentRootPattern();

    [GeneratedRegex(@"^(.+)/")]
    private static partial Regex ExtractParentTopicPathPattern();

    /// <summary>
    /// Gets the parent topic path for a given topic path.
    /// </summary>
    /// <param name="topicPath">Topic path.</param>
    /// <returns>
    ///   <para>The parent topic path if the topic has a parent; otherwise, null.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    /// </exception>
    public static string? GetParentTopicPath(string topicPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicPath, nameof(topicPath));

        if (topicPath == "/") {
            return null;
        }

        if (IsParentRootPattern().IsMatch(topicPath)) {
            return "/";
        }

        var parentMatch = ExtractParentTopicPathPattern().Match(topicPath);
        return parentMatch.Success
            ? parentMatch.Groups[1].Value
            : null;
    }

    [GeneratedRegex(@"(^index|/index)?(\.md)?$")]
    private static partial Regex IndexTopicPathPattern();

    /// <summary>
    /// Resolves the given topic path.
    /// </summary>
    /// <remarks>
    ///   <para>The following things are resolved:</para>
    ///   <list type="bullet">
    ///     <item>Ensure that path starts with '/'.</item>
    ///     <item>Always uses forward-slash characters; for example, '\' -> '/'.</item>
    ///     <item>Index topic paths; for example, "/section/index" -> "/section".</item>
    ///     <item>Remove trailing ".md" file extension if present.</item>
    ///     <item>Remove trailing forward-slash character; for example, "/section/" -> "/section".</item>
    ///   </list>
    /// </remarks>
    /// <param name="topicPath">Topic path.</param>
    /// <returns>
    ///   <para>The resolved topic path.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="topicPath"/> is null.</para>
    /// </exception>
    public static string ResolveTopicPath(string topicPath)
    {
        ExceptionHelpers.ThrowIfArgumentNull(topicPath, nameof(topicPath));

        topicPath = topicPath.Replace('\\', '/');

        topicPath = IndexTopicPathPattern().Replace(topicPath, "");

        if (!topicPath.StartsWith('/')) {
            topicPath = $"/{topicPath}";
        }
        if (topicPath.Length > 1 && topicPath.EndsWith('/')) {
            topicPath = topicPath.TrimEnd('/');
        }

        return topicPath;
    }
}
