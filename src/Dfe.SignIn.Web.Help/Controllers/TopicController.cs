using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Models;
using Dfe.SignIn.WebFramework.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Help.Controllers;

/// <summary>
/// The controller for a help topic page.
/// </summary>
public sealed class TopicController(
    ITopicIndexAccessor topicIndexAccessor
) : Controller
{
    public async Task<IActionResult> Index()
    {
        var topicIndex = await topicIndexAccessor.GetIndexAsync();

        string topicPath = this.Request.Path;
        var topic = topicIndex.GetTopic(topicPath ?? "/");
        if (topic is null) {
            return this.NotFound();
        }

        var crumbs = topicIndex.GetCrumbs(topic).Select(crumbTopic => new CrumbViewModel {
            Text = crumbTopic.Metadata.NavigationTitle,
            Href = new Uri(crumbTopic.Path, UriKind.Relative),
        });

        var cardSections = topic.Metadata.Topics.Select(topicListing => new CardSectionViewModel {
            Heading = topicListing.Heading,
            Cards = topicListing.Paths.Select(childTopicPath => {
                var childTopic = topicIndex.GetRequiredTopic(childTopicPath);
                return new CardViewModel {
                    Title = childTopic.Metadata.NavigationTitle,
                    Summary = childTopic.Metadata.Summary,
                    Href = new Uri(childTopic.Path, UriKind.Relative),
                };
            }),
        });

        return this.View(new TopicViewModel {
            AllowDeveloperReloadAction = AllowDeveloperReloadAction,
            Crumbs = crumbs,
            Caption = topic.Metadata.Caption,
            Title = topic.Metadata.Title,
            Summary = topic.Metadata.Summary,
            ContentHtml = topic.ContentHtml,
            CardSections = cardSections,
        });
    }

    #region Developer reload feature

    /// <summary>
    /// Gets a value indicating if the developer "Reload" action should be presented.
    /// </summary>
#if DEBUG
    public static bool AllowDeveloperReloadAction => true;
#else
    public static bool AllowDeveloperReloadAction => false;
#endif

#if DEBUG
    /// <summary>
    /// This endpoint forces a reload of the topic index.
    /// </summary>
    public async Task<IActionResult> Reload()
    {
        await topicIndexAccessor.GetIndexAsync(invalidate: true);
        return this.Ok();
    }
#endif

    #endregion
}
