using Dfe.SignIn.Web.Help.Content;

namespace Dfe.SignIn.Web.Help.UnitTests.Content;

[TestClass]
public sealed class TopicMemoryCacheTests
{
    public static readonly TopicModel[] FakeTopics = [
        new() {
            Path = "/",
            Metadata = new() {
                Title = "Home page",
                NavigationTitle = "Home",
            },
            ContentHtml = "Example content...",
        },
        new() {
            Path = "/my-account",
            Metadata = new() {
                Title = "My account",
                NavigationTitle = "My account",
            },
            ContentHtml = "Example content...",
        },
        new() {
            Path = "/my-account/changing-my-password",
            Metadata = new() {
                Title = "Changing my password",
                NavigationTitle = "Changing my password",
            },
            ContentHtml = "Example content...",
        },
        new() {
            Path = "/my-account/multifactor-authentication",
            Metadata = new() {
                Title = "Multifactor authentication",
                NavigationTitle = "Multifactor authentication",
            },
            ContentHtml = "Example content...",
        },
    ];

    #region GetAllTopics()

    [TestMethod]
    public void GetAllTopics_ReturnsAllTopics()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var topics = topicCache.GetAllTopics();

        Assert.IsNotNull(topics);
        Assert.AreEqual(FakeTopics.Length, topics.Count());
        CollectionAssert.AreEquivalent(FakeTopics, topics.ToArray());
    }

    #endregion

    #region GetTopic(string)

    [TestMethod]
    public void GetTopic_Throws_WhenTopicPathArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        Assert.ThrowsExactly<ArgumentNullException>(()
            => topicCache.GetTopic(null!));
    }

    [TestMethod]
    public void GetTopic_ReturnsNull_WhenTopicIsNotFound()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var topic = topicCache.GetTopic("/unexpected-path");

        Assert.IsNull(topic);
    }

    [TestMethod]
    public void GetTopic_ReturnsExpectedTopic()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var expectedFakeTopic = FakeTopics.First(fakeTopic => fakeTopic.Path == "/my-account/changing-my-password");
        var topic = topicCache.GetTopic(expectedFakeTopic.Path);

        Assert.IsNotNull(topic);
        Assert.AreSame(expectedFakeTopic, topic);
    }

    #endregion

    #region GetParentTopic(string)

    [TestMethod]
    public void GetParentTopic_Throws_WhenTopicPathArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        Assert.ThrowsExactly<ArgumentNullException>(()
            => topicCache.GetParentTopic(null!));
    }

    [TestMethod]
    public void GetParentTopic_ReturnsNull_WhenTopicDoesNotHaveParent()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var parentTopic = topicCache.GetParentTopic("/");

        Assert.IsNull(parentTopic);
    }

    [TestMethod]
    public void GetParentTopic_ReturnsNull_WhenTopicDoesNotExist()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var parentTopic = topicCache.GetParentTopic("/unexpected/path");

        Assert.IsNull(parentTopic);
    }

    [DataRow("/my-account", "/")]
    [DataRow("/my-account/changing-my-password", "/my-account")]
    [DataTestMethod]
    public void GetParentTopic_ReturnsExpectedParentTopic(string topicPath, string expectedParentTopicPath)
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var parentTopic = topicCache.GetParentTopic(topicPath);

        var expectedParentTopic = FakeTopics.First(fakeTopic => fakeTopic.Path == expectedParentTopicPath);
        Assert.IsNotNull(parentTopic);
        Assert.AreSame(expectedParentTopic, parentTopic);
    }

    #endregion

    #region GetChildTopics(string)

    [TestMethod]
    public void GetChildTopics_Throws_WhenTopicPathArgumentIsNull()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        Assert.ThrowsExactly<ArgumentNullException>(()
            => topicCache.GetChildTopics(null!));
    }

    [TestMethod]
    public void GetChildTopics_ReturnsEmptyArray_WhenTopicDoesNotExist()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var childTopics = topicCache.GetChildTopics("/unexpected-topic");

        Assert.IsNotNull(childTopics);
        Assert.AreEqual(0, childTopics.Count());
    }

    [TestMethod]
    public void GetChildTopics_ReturnsEmptyArray_WhenTopicDoesNotHaveAnyChildren()
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var childTopics = topicCache.GetChildTopics("/my-account/changing-my-password");

        Assert.IsNotNull(childTopics);
        Assert.AreEqual(0, childTopics.Count());
    }

    [DataRow("/", new string[] { "/my-account" })]
    [DataRow("/my-account", new string[] { "/my-account/changing-my-password", "/my-account/multifactor-authentication" })]
    [DataTestMethod]
    public void GetChildTopics_ReturnsExpectedChildTopics(string topicPath, string[] expectedChildTopicPaths)
    {
        var topicCache = new TopicMemoryCache(FakeTopics);

        var childTopics = topicCache.GetChildTopics(topicPath);

        var expectedChildTopics = FakeTopics.Where(fakeTopic => expectedChildTopicPaths.Contains(fakeTopic.Path));
        Assert.IsNotNull(childTopics);
        Assert.AreEqual(expectedChildTopicPaths.Length, childTopics.Count());
        CollectionAssert.AreEquivalent(expectedChildTopics.ToArray(), childTopics.ToArray());
    }

    #endregion
}
