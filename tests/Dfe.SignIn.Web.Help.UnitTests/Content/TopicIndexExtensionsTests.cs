using Dfe.SignIn.Web.Help.Content;

namespace Dfe.SignIn.Web.Help.UnitTests.Content;

[TestClass]
public sealed class TopicIndexExtensionsTests
{
    #region GetRequiredTopic(this ITopicIndex topicIndex, string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetRequiredTopic_Throws_WhenTopicIndexArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        TopicIndexExtensions.GetRequiredTopic(null!, "/");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetRequiredTopic_Throws_WhenTopicPathArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        TopicIndexExtensions.GetRequiredTopic(topicCache, null!);
    }

    [TestMethod]
    public void GetRequiredTopic_Throws_WhenTopicIsNotFound()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var exception = Assert.Throws<KeyNotFoundException>(
            () => TopicIndexExtensions.GetRequiredTopic(topicCache, "/unexpected-path")
        );

        Assert.AreEqual("Topic is missing '/unexpected-path'.", exception.Message);
    }

    [TestMethod]
    public void GetRequiredTopic_ReturnsExpectedTopic()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var expectedTopic = TopicMemoryCacheTests.FakeTopics.First(
            fakeTopic => fakeTopic.Path == "/my-account/changing-my-password"
        );

        var topic = TopicIndexExtensions.GetRequiredTopic(topicCache, expectedTopic.Path);

        Assert.IsNotNull(topic);
        Assert.AreSame(expectedTopic, topic);
    }

    #endregion

    #region GetParentTopic(this ITopicIndex topicIndex, TopicModel)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetParentTopic_Throws_WhenTopicIndexArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var topic = TopicMemoryCacheTests.FakeTopics.First();
        TopicIndexExtensions.GetParentTopic(null!, topic);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetParentTopic_Throws_WhenTopicArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        TopicIndexExtensions.GetParentTopic(topicCache, null!);
    }

    [TestMethod]
    public void GetParentTopic_ReturnsNull_WhenTopicDoesNotHaveParent()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var rootTopic = TopicMemoryCacheTests.FakeTopics.First(fakeTopic => fakeTopic.Path == "/");
        var parentTopic = TopicIndexExtensions.GetParentTopic(topicCache, rootTopic);

        Assert.IsNull(parentTopic);
    }

    [DataRow("/my-account", "/")]
    [DataRow("/my-account/changing-my-password", "/my-account")]
    [DataTestMethod]
    public void GetParentTopic_ReturnsExpectedParentTopic(string topicPath, string expectedParentTopicPath)
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var topic = TopicMemoryCacheTests.FakeTopics.First(fakeTopic => fakeTopic.Path == topicPath);
        var parentTopic = TopicIndexExtensions.GetParentTopic(topicCache, topic);

        var expectedParentTopic = TopicMemoryCacheTests.FakeTopics.First(fakeTopic => fakeTopic.Path == expectedParentTopicPath);
        Assert.IsNotNull(parentTopic);
        Assert.AreSame(expectedParentTopic, parentTopic);
    }

    #endregion

    #region GetChildTopics(this ITopicIndex topicIndex, TopicModel)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetChildTopics_Throws_WhenTopicIndexArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var topic = TopicMemoryCacheTests.FakeTopics.First();
        TopicIndexExtensions.GetChildTopics(null!, topic);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetChildTopics_Throws_WhenTopicArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        TopicIndexExtensions.GetChildTopics(topicCache, null!);
    }

    [TestMethod]
    public void GetChildTopics_ReturnsEmptyArray_WhenTopicDoesNotHaveAnyChildren()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var topic = TopicMemoryCacheTests.FakeTopics.First(fakeTopic => fakeTopic.Path == "/my-account/changing-my-password");
        var childTopics = TopicIndexExtensions.GetChildTopics(topicCache, topic);

        Assert.IsNotNull(childTopics);
        Assert.AreEqual(0, childTopics.Count());
    }

    [DataRow("/", new string[] { "/my-account" })]
    [DataRow("/my-account", new string[] { "/my-account/changing-my-password", "/my-account/multifactor-authentication" })]
    [DataTestMethod]
    public void GetChildTopics_ReturnsExpectedChildTopics(string topicPath, string[] expectedChildTopicPaths)
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var topic = TopicMemoryCacheTests.FakeTopics.First(fakeTopic => fakeTopic.Path == topicPath);
        var childTopics = TopicIndexExtensions.GetChildTopics(topicCache, topic);

        Assert.IsNotNull(childTopics);
        Assert.AreEqual(expectedChildTopicPaths.Length, childTopics.Count());

        var expectedChildTopics = TopicMemoryCacheTests.FakeTopics.Where(fakeTopic => expectedChildTopicPaths.Contains(fakeTopic.Path));
        CollectionAssert.AreEquivalent(expectedChildTopics.ToArray(), childTopics.ToArray());
    }

    #endregion

    #region GetCrumbs(this ITopicIndex topicIndex, TopicModel)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetCrumbs_Throws_WhenTopicIndexArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var topic = TopicMemoryCacheTests.FakeTopics.First();
        TopicIndexExtensions.GetCrumbs(null!, topic);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetCrumbs_Throws_WhenTopicArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        TopicIndexExtensions.GetCrumbs(topicCache, null!);
    }

    [DataRow("/", new string[0])]
    [DataRow("/my-account", new string[] { "/" })]
    [DataRow("/my-account/changing-my-password", new string[] { "/", "/my-account" })]
    [DataTestMethod]
    public void GetCrumbs_ReturnsExpectedCrumbTopics(string topicPath, string[] expectedCrumbTopicPaths)
    {
        var topicCache = new TopicMemoryCache(TopicMemoryCacheTests.FakeTopics);

        var topic = TopicMemoryCacheTests.FakeTopics.First(fakeTopic => fakeTopic.Path == topicPath);
        var crumbs = TopicIndexExtensions.GetCrumbs(topicCache, topic);

        Assert.IsNotNull(crumbs);

        string[] crumbTopicPaths = [.. crumbs.Select(crumb => crumb.Path)];
        CollectionAssert.AreEqual(expectedCrumbTopicPaths, crumbTopicPaths);
    }

    #endregion
}
