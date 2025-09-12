using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Content.Processing;
using HtmlAgilityPack;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Help.UnitTests.Content;

[TestClass]
public sealed class TopicFilePipelineTests
{
    private const string FakeTopicFileContent = "Example content...";

    private static void SetupFakeFileReader(AutoMocker autoMocker)
    {
        autoMocker.GetMock<ITopicFileReader>()
            .Setup(x =>
                x.ReadAsync(
                    It.Is<string>(topicFilePath => topicFilePath == "./ContentFiles/my-account/index.md"),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(FakeTopicFileContent);
    }

    private sealed class FakeTopicProcessor : ITopicProcessor
    {
        public Task<TopicModel> ProcessAsync(TopicModel topic, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                topic with {
                    Metadata = topic.Metadata with {
                        Title = "Override title",
                    },
                }
            );
        }
    }

    #region LoadAllTopicFilesAsync(string, CancellationToken)

    [TestMethod]
    public Task LoadAllTopicFilesAsync_Throws_WhenContentFilesPathArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        return Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => pipeline.LoadAllTopicFilesAsync(null!));
    }

    [TestMethod]
    public Task LoadAllTopicFilesAsync_Throws_WhenContentFilesPathArgumentIsEmpty()
    {
        var autoMocker = new AutoMocker();

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        return Assert.ThrowsExactlyAsync<ArgumentException>(()
            => pipeline.LoadAllTopicFilesAsync(""));
    }

    #endregion

    #region LoadTopicFileAsync(string, CancellationToken)

    [TestMethod]
    public Task LoadTopicFileAsync_Throws_WhenContentFilesPathArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        return Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => pipeline.LoadTopicFileAsync(null!, "/"));
    }

    [TestMethod]
    public Task LoadTopicFileAsync_Throws_WhenContentFilesPathArgumentIsEmpty()
    {
        var autoMocker = new AutoMocker();

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        return Assert.ThrowsExactlyAsync<ArgumentException>(()
            => pipeline.LoadTopicFileAsync("", "/"));
    }

    [TestMethod]
    public Task LoadTopicFileAsync_Throws_WhenTopicFilePathArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        return Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => pipeline.LoadTopicFileAsync(".", null!));
    }

    [TestMethod]
    public Task LoadTopicFileAsync_Throws_WhenTopicFilePathArgumentIsEmpty()
    {
        var autoMocker = new AutoMocker();

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        return Assert.ThrowsExactlyAsync<ArgumentException>(()
            => pipeline.LoadTopicFileAsync(".", ""));
    }

    [TestMethod]
    public async Task LoadTopicFileAsync_ReadsRequestedTopicFile()
    {
        var autoMocker = new AutoMocker();
        SetupFakeFileReader(autoMocker);
        autoMocker.Use<ITopicMarkdownProcessor>(new MarkdigTopicMarkdownProcessor());
        autoMocker.Use<IEnumerable<ITopicPreProcessor>>([]);
        autoMocker.Use<IEnumerable<ITopicProcessor>>([]);

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        var topic = await pipeline.LoadTopicFileAsync("./ContentFiles", "./ContentFiles/my-account/index.md");

        Assert.IsNotNull(topic);
        Assert.AreEqual("My account", topic.Metadata.Title);

        var doc = new HtmlDocument();
        doc.LoadHtml(topic.ContentHtml);
        var paragraph = doc.DocumentNode.Descendants("p").FirstOrDefault();
        Assert.IsNotNull(paragraph);
        Assert.AreEqual(FakeTopicFileContent, paragraph.InnerText);
    }

    [TestMethod]
    public async Task LoadTopicFileAsync_PreProcessesMarkdownContent()
    {
        var autoMocker = new AutoMocker();
        SetupFakeFileReader(autoMocker);
        autoMocker.Use<ITopicMarkdownProcessor>(new MarkdigTopicMarkdownProcessor());
        autoMocker.Use<IEnumerable<ITopicProcessor>>([]);

        autoMocker.GetMock<ITopicPreProcessor>()
            .Setup(x =>
                x.ProcessAsync(
                    It.Is<string>(topicPath => topicPath == "/my-account"),
                    It.Is<string>(markdown => markdown == FakeTopicFileContent),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync("Modified content...");

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        var topic = await pipeline.LoadTopicFileAsync("./ContentFiles", "./ContentFiles/my-account/index.md");

        Assert.IsNotNull(topic);
        Assert.AreEqual("My account", topic.Metadata.Title);

        var doc = new HtmlDocument();
        doc.LoadHtml(topic.ContentHtml);
        var paragraph = doc.DocumentNode.Descendants("p").FirstOrDefault();
        Assert.IsNotNull(paragraph);
        Assert.AreEqual("Modified content...", paragraph.InnerText);
    }

    [TestMethod]
    public async Task LoadTopicFileAsync_ProcessesTopic()
    {
        var autoMocker = new AutoMocker();
        SetupFakeFileReader(autoMocker);
        autoMocker.Use<ITopicMarkdownProcessor>(new MarkdigTopicMarkdownProcessor());
        autoMocker.Use<IEnumerable<ITopicPreProcessor>>([]);
        autoMocker.Use<ITopicProcessor>(new FakeTopicProcessor());

        var pipeline = autoMocker.CreateInstance<TopicFilePipeline>();

        var topic = await pipeline.LoadTopicFileAsync("./ContentFiles", "./ContentFiles/my-account/index.md");

        Assert.IsNotNull(topic);
        Assert.AreEqual("Override title", topic.Metadata.Title);
    }

    #endregion
}
