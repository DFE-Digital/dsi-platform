using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.WebFramework.Models;

namespace Dfe.SignIn.Web.Help.Services;

/// <summary>
/// Represents a service that build the service navigation menu.
/// </summary>
public interface IServiceNavigationBuilder
{
    /// <summary>
    /// Build service navigation menu.
    /// </summary>
    /// <param name="currentPath">The current request path; for example, "/contact-us".</param>
    /// <returns>
    ///   <para>An enumerable collection of zero-or-more service navigation menu items.</para>
    /// </returns>
    Task<IEnumerable<NavigationItemViewModel>> BuildAsync(string currentPath);
}

/// <summary>
/// The service navigation builder service.
/// </summary>
/// <param name="topicIndexAccessor">A service that provides access to the topic index.</param>
public sealed class ServiceNavigationBuilder(
    ITopicIndexAccessor topicIndexAccessor
) : IServiceNavigationBuilder
{
    /// <inheritdoc/>
    public async Task<IEnumerable<NavigationItemViewModel>> BuildAsync(string currentPath)
    {
        // Do not present any service navigation items on the home page since the page
        // presents more prominent navigation.
        if (currentPath == "/") {
            return [];
        }

        // Infer section navigation from first topic listing section of the home page.
        return (await this.GetRootSectionTopicsAsync())
            .Select(sectionTopic => new NavigationItemViewModel {
                Text = sectionTopic.Metadata.NavigationTitle,
                Href = new Uri(sectionTopic.Path, UriKind.Relative),
                IsActive = currentPath.StartsWith(sectionTopic.Path),
            });
    }

    private async Task<IEnumerable<TopicModel>> GetRootSectionTopicsAsync()
    {
        var topicIndex = await topicIndexAccessor.GetIndexAsync();

        var rootTopic = topicIndex.GetRequiredTopic("/");
        var rootSectionPaths = rootTopic.Metadata.Topics.FirstOrDefault()?.Paths ?? [];
        return rootSectionPaths.Select(topicIndex.GetRequiredTopic);
    }
}
