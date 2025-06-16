using Dfe.SignIn.Web.Help.Content;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Help.UnitTests.Content;

[TestClass]
public sealed class TopicMemoryCacheAccessorTests
{
    #region GetIndexAsync(bool)

    [TestMethod]
    public async Task GetIndexAsync_LoadsAllTopicFilesOnFirstAccess()
    {
        var autoMocker = new AutoMocker();
        var accessor = autoMocker.CreateInstance<TopicMemoryCacheAccessor>();

        var topicIndex = await accessor.GetIndexAsync(invalidate: false);

        string expectedContentFilesPath = Path.Join(Directory.GetCurrentDirectory(), "ContentFiles");

        Assert.IsNotNull(topicIndex);

        autoMocker.Verify<ITopicFilePipeline>(pipeline =>
            pipeline.LoadAllTopicFilesAsync(
                It.Is<string>(contentFilesPath => contentFilesPath == expectedContentFilesPath),
                It.IsAny<CancellationToken>()
            ),
            Times.Once()
        );
    }

    [TestMethod]
    public async Task GetIndexAsync_ReturnsIndexOfCachedTopics()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<ITopicFilePipeline>()
            .Setup(pipeline =>
                pipeline.LoadAllTopicFilesAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([
                new TopicModel {
                    Path = "/fake-topic",
                    Metadata = new() {
                        Title = "Fake topic",
                        NavigationTitle = "Fake topic",
                    },
                    ContentHtml = "<p>Fake topic content.</p>",
                },
            ]);

        var accessor = autoMocker.CreateInstance<TopicMemoryCacheAccessor>();

        var topicIndex = await accessor.GetIndexAsync(invalidate: false);

        var topic = topicIndex.GetTopic("/fake-topic");
        Assert.IsNotNull(topic);
        Assert.AreEqual("Fake topic", topic.Metadata.Title);
    }

    [TestMethod]
    public async Task GetIndexAsync_DoesNotLoadTopicFilesOnSubsequentAccesses()
    {
        var autoMocker = new AutoMocker();
        var accessor = autoMocker.CreateInstance<TopicMemoryCacheAccessor>();

        var topicIndex1 = await accessor.GetIndexAsync(invalidate: false);
        var topicIndex2 = await accessor.GetIndexAsync(invalidate: false);

        string expectedContentFilesPath = Path.Join(Directory.GetCurrentDirectory(), "ContentFiles");

        Assert.AreSame(topicIndex1, topicIndex2);

        autoMocker.Verify<ITopicFilePipeline>(pipeline =>
            pipeline.LoadAllTopicFilesAsync(
                It.Is<string>(contentFilesPath => contentFilesPath == expectedContentFilesPath),
                It.IsAny<CancellationToken>()
            ),
            Times.Once()
        );
    }

    [TestMethod]
    public async Task GetIndexAsync_ReloadsTopicFiles_WhenInvalidateArgumentIsTrue()
    {
        var autoMocker = new AutoMocker();
        var accessor = autoMocker.CreateInstance<TopicMemoryCacheAccessor>();

        var topicIndex1 = await accessor.GetIndexAsync(invalidate: false);
        var topicIndex2 = await accessor.GetIndexAsync(invalidate: true);

        string expectedContentFilesPath = Path.Join(Directory.GetCurrentDirectory(), "ContentFiles");

        Assert.AreNotSame(topicIndex1, topicIndex2);

        autoMocker.Verify<ITopicFilePipeline>(pipeline =>
            pipeline.LoadAllTopicFilesAsync(
                It.Is<string>(contentFilesPath => contentFilesPath == expectedContentFilesPath),
                It.IsAny<CancellationToken>()
            ),
            Times.Exactly(2)
        );
    }

    #endregion
}
