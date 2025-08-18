using Dfe.SignIn.WebFramework.Models;

namespace Dfe.SignIn.Web.Help.Models;

/// <summary>
/// Represents the view model of a help topic page.
/// </summary>
public sealed class TopicViewModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the reload action should be shown.
    /// </summary>
    /// <remarks>
    ///   <para>This is a developer feature which is useful when running the application
    ///   locally. A developer can use this feature to reload the static content cache
    ///   allowing them to preview content changes without restarting the application.</para>
    /// </remarks>
    public bool AllowDeveloperReloadAction { get; set; } = false;

    /// <summary>
    /// Gets or sets the breadcrumbs of topics leading to the current one.
    /// </summary>
    /// <remarks>
    ///   <para>This is an ordered collection of zero-or-more crumbs.</para>
    /// </remarks>
    public IEnumerable<CrumbViewModel> Crumbs { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional caption of the topic.
    /// </summary>
    public required string? Caption { get; set; }

    /// <summary>
    /// Gets or sets the title of the topic.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the optional brief summary of the topic.
    /// </summary>
    /// <remarks>
    ///   <para>This is plain text which should be rendered into an appropriate HTML
    ///   element such as a paragraph.</para>
    ///   <para>This text can also be used for description meta tags.</para>
    /// </remarks>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the HTML encoded body content of the topic.
    /// </summary>
    /// <remarks>
    ///   <para>This content should be presented as raw HTML since it is already HTML
    ///   encoded.</para>
    /// </remarks>
    public required string ContentHtml { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the topic is a global topic.
    /// </summary>
    /// <remarks>
    ///   <para>Global "non-help" topics such as the "Cookies" page are presented within
    ///   the "DfE Sign-in" space and do not include service navigation links.</para>
    ///   <para>Help topics are presented within the "DfE Sign-in Help" space and include
    ///   the related service navigation links.</para>
    /// </remarks>
    public bool IsGlobal { get; set; } = false;

    /// <summary>
    /// Gets or sets any topic listing sections.
    /// </summary>
    /// <remarks>
    ///   <para>This is a ordered collection of zero-or-more card sections.</para>
    /// </remarks>
    public IEnumerable<TopicListingViewModel> TopicListingSections { get; set; } = [];
}
