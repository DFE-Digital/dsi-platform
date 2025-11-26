using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Models;
using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Help.Controllers;

/// <summary>
/// The controller for a help topic page.
/// </summary>
public sealed class TopicController(
    ITopicIndexAccessor topicIndexAccessor
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var topicIndex = await topicIndexAccessor.GetIndexAsync();

        string topicPath = this.Request.Path;
        var topic = topicIndex.GetTopic(topicPath ?? "/");
        if (topic is null) {
            return this.NotFound();
        }

        var crumbs = topicIndex.GetCrumbs(topic);
        // Skip first crumb for global "non-help" topics (eg. "Cookies").
        if (topic.Metadata.IsGlobal) {
            crumbs = crumbs.Skip(1);
        }

        return this.View(new TopicViewModel {
            AllowDeveloperReloadAction = AllowDeveloperReloadAction,
            Updated = topic.Metadata.Updated,
            Crumbs = BuildCrumbViewModels(crumbs),
            Caption = topic.Metadata.Caption,
            Title = topic.Metadata.Title,
            Summary = topic.Metadata.Summary,
            ContentHtml = topic.ContentHtml,
            IsGlobal = topic.Metadata.IsGlobal,
            TopicListingSections = topic.Metadata.Topics.Select(
                topicListing => BuildTopicListingViewModel(topicListing, topicIndex)
            ),
        });
    }

    private static IEnumerable<CrumbViewModel> BuildCrumbViewModels(IEnumerable<TopicModel> crumbs)
    {
        return crumbs.Select(crumbTopic => new CrumbViewModel {
            Text = crumbTopic.Metadata.NavigationTitle,
            Href = new Uri(crumbTopic.Path, UriKind.Relative),
        });
    }

    private static TopicListingViewModel BuildTopicListingViewModel(TopicListing topicListing, ITopicIndex topicIndex)
    {
        return new TopicListingViewModel {
            Heading = topicListing.Heading,
            Topics = topicListing.Paths.Select(childTopicPath => {
                var childTopic = topicIndex.GetRequiredTopic(childTopicPath);
                return BuildTopicListingItemViewModel(childTopic);
            }),
        };
    }

    private static TopicListingItemViewModel BuildTopicListingItemViewModel(TopicModel topic)
    {
        return new TopicListingItemViewModel {
            Title = topic.Metadata.NavigationTitle,
            Summary = topic.Metadata.Summary,
            Href = new Uri(topic.Path, UriKind.Relative),
        };
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
    [HttpGet]
    public async Task<IActionResult> Reload()
    {
        await topicIndexAccessor.GetIndexAsync(invalidate: true);
        return this.Ok();
    }
#endif

    #endregion
}
