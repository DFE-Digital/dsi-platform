using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Services;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Help.UnitTests.Services;

[TestClass]
public sealed class ServiceNavigationBuilderTests
{
    private static readonly TopicModel HomePageTopic = new() {
        Path = "/",
        Metadata = new() {
            Title = "Home page",
            NavigationTitle = "Home",
            Topics = [
                new() {
                    Paths = [
                        "/getting-started",
                        "/guidance",
                    ],
                },
            ],
        },
        ContentHtml = "<p>Welcome!</p>",
    };

    private static readonly TopicModel GettingStartedTopic = new() {
        Path = "/getting-started",
        Metadata = new() {
            Title = "Getting started with the service",
            NavigationTitle = "Getting started",
        },
        ContentHtml = "<p>Getting started...</p>",
    };

    private static readonly TopicModel GuidanceTopic = new() {
        Path = "/guidance",
        Metadata = new() {
            Title = "General guidance",
            NavigationTitle = "Guidance",
        },
        ContentHtml = "<p>Guidance...</p>",
    };

    private static readonly TopicModel ExampleTopic = new() {
        Path = "/guidance/example",
        Metadata = new() {
            Title = "Example topic",
            NavigationTitle = "Example topic",
        },
        ContentHtml = "<p>An example topic...</p>",
    };

    private static void SetupTopicIndex(AutoMocker autoMocker)
    {
        var mockTopicIndex = autoMocker.GetMock<ITopicIndex>();

        autoMocker.GetMock<ITopicIndexAccessor>()
            .Setup(mock => mock.GetIndexAsync(
                It.Is<bool>(invalidate => !invalidate)
            ))
            .ReturnsAsync(mockTopicIndex.Object);

        mockTopicIndex
            .Setup(mock => mock.GetTopic(
                It.Is<string>(topicPath => topicPath == "/")
            ))
            .Returns(HomePageTopic);

        mockTopicIndex
            .Setup(mock => mock.GetTopic(
                It.Is<string>(topicPath => topicPath == "/getting-started")
            ))
            .Returns(GettingStartedTopic);

        mockTopicIndex
            .Setup(mock => mock.GetTopic(
                It.Is<string>(topicPath => topicPath == "/guidance")
            ))
            .Returns(GuidanceTopic);

        mockTopicIndex
            .Setup(mock => mock.GetTopic(
                It.Is<string>(topicPath => topicPath == "/guidance/example")
            ))
            .Returns(ExampleTopic);
    }

    #region BuildAsync(string)

    [TestMethod]
    public async Task BuildAsync_ReturnsEmptyCollection_WhenCurrentPathIsHomePage()
    {
        var autoMocker = new AutoMocker();
        SetupTopicIndex(autoMocker);
        var builder = autoMocker.CreateInstance<ServiceNavigationBuilder>();

        var result = await builder.BuildAsync("/");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public async Task BuildAsync_ReturnsExpectedItems()
    {
        var autoMocker = new AutoMocker();
        SetupTopicIndex(autoMocker);
        var builder = autoMocker.CreateInstance<ServiceNavigationBuilder>();

        var result = await builder.BuildAsync("/getting-started");

        Assert.HasCount(2, result);

        var item1 = result.ElementAt(0);
        Assert.AreEqual("Getting started", item1.Text);
        Assert.IsTrue(item1.IsActive);
        Assert.AreEqual(new Uri("/getting-started", UriKind.Relative), item1.Href);

        var item2 = result.ElementAt(1);
        Assert.AreEqual("Guidance", item2.Text);
        Assert.IsFalse(item2.IsActive);
        Assert.AreEqual(new Uri("/guidance", UriKind.Relative), item2.Href);
    }

    [TestMethod]
    public async Task BuildAsync_MarksCurrentPathAsActive_WhenViewingRootTopic()
    {
        var autoMocker = new AutoMocker();
        SetupTopicIndex(autoMocker);
        var builder = autoMocker.CreateInstance<ServiceNavigationBuilder>();

        var result = await builder.BuildAsync("/guidance");

        var item2 = result.ElementAt(1);
        Assert.AreEqual("Guidance", item2.Text);
        Assert.IsTrue(item2.IsActive);
        Assert.AreEqual(new Uri("/guidance", UriKind.Relative), item2.Href);
    }

    [TestMethod]
    public async Task BuildAsync_MarksCurrentPathAsActive_WhenViewingNestedTopic()
    {
        var autoMocker = new AutoMocker();
        SetupTopicIndex(autoMocker);
        var builder = autoMocker.CreateInstance<ServiceNavigationBuilder>();

        var result = await builder.BuildAsync("/guidance/example");

        var item2 = result.ElementAt(1);
        Assert.AreEqual("Guidance", item2.Text);
        Assert.IsTrue(item2.IsActive);
        Assert.AreEqual(new Uri("/guidance", UriKind.Relative), item2.Href);
    }

    #endregion
}
