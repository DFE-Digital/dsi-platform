using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Controllers;
using Dfe.SignIn.Web.Help.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.Help.UnitTests.Controllers;

[TestClass]
public sealed class TopicControllerTests
{
    private static readonly TopicModel HomePageTopic = new() {
        Path = "/",
        Metadata = new() {
            Title = "Home page",
            NavigationTitle = "Home",
        },
        ContentHtml = "<p>Welcome!</p>",
    };

    private static readonly TopicModel GuidanceSectionTopic = new() {
        Path = "/guidance",
        Metadata = new() {
            Title = "Guidance",
            NavigationTitle = "Guidance",
            Topics = [
                new() {
                    Heading = "Topics in this section",
                    Paths = [
                        "/guidance/example-topic",
                    ],
                },
            ],
        },
        ContentHtml = "<p>Find guidance in the following sections.</p>",
    };

    private static readonly TopicModel ExampleTopic = new() {
        Path = "/guidance/example-topic",
        Metadata = new() {
            Caption = "Example content",
            Title = "Example topic",
            NavigationTitle = "Example",
            Summary = "Summary of example topic.",
        },
        ContentHtml = "<p>An example topic.</p>",
    };

    private static readonly TopicModel ExampleGlobalTopic = new() {
        Path = "/cookies",
        Metadata = new() {
            Title = "Details about cookies",
            NavigationTitle = "Cookies",
            IsGlobal = true,
        },
        ContentHtml = "<p>Cookie details...</p>",
    };

    private static Mock<ITopicIndex> SetupTopicIndex(AutoMocker autoMocker)
    {
        var mockTopicIndex = autoMocker.GetMock<ITopicIndex>();

        autoMocker.GetMock<ITopicIndexAccessor>()
            .Setup(mock => mock.GetIndexAsync(
                It.Is<bool>(invalidate => !invalidate)
            ))
            .ReturnsAsync(mockTopicIndex.Object);

        mockTopicIndex
            .Setup(mock => mock.GetTopic(
                It.Is<string>(topicPath => topicPath == "/guidance")
            ))
            .Returns(GuidanceSectionTopic);

        mockTopicIndex
            .Setup(mock => mock.GetTopic(
                It.Is<string>(topicPath => topicPath == "/guidance/example-topic")
            ))
            .Returns(ExampleTopic);

        mockTopicIndex
            .Setup(mock => mock.GetTopic(
                It.Is<string>(topicPath => topicPath == "/cookies")
            ))
            .Returns(ExampleGlobalTopic);

        return mockTopicIndex;
    }

    private static TopicController CreateController(AutoMocker autoMocker, string requestPath)
    {
        var controller = autoMocker.CreateInstance<TopicController>();

        var mockRequest = autoMocker.GetMock<HttpRequest>();
        mockRequest.Setup(mock => mock.Path).Returns(requestPath);

        var mockContext = autoMocker.GetMock<HttpContext>();
        mockContext.Setup(mock => mock.Request).Returns(mockRequest.Object);

        controller.ControllerContext = new ControllerContext {
            HttpContext = mockContext.Object,
        };

        return controller;
    }

    [TestMethod]
    public async Task Index_ReturnsNotFound_WhenTopicIsNotFound()
    {
        var autoMocker = new AutoMocker();
        var mockTopicIndex = SetupTopicIndex(autoMocker);

        var controller = CreateController(autoMocker, "/non-existing/topic");

        var result = await controller.Index();

        TypeAssert.IsType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Index_PresentsTopicContent()
    {
        var autoMocker = new AutoMocker();
        SetupTopicIndex(autoMocker);

        var controller = CreateController(autoMocker, "/guidance/example-topic");

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<TopicViewModel>(result);
        Assert.AreEqual("Example content", viewModel.Caption);
        Assert.AreEqual("Example topic", viewModel.Title);
        Assert.AreEqual("<p>An example topic.</p>", viewModel.ContentHtml);
        Assert.IsEmpty(viewModel.TopicListingSections);
    }

    [TestMethod]
    public async Task Index_PresentsAllCrumbs()
    {
        var autoMocker = new AutoMocker();
        var mockTopicIndex = SetupTopicIndex(autoMocker);

        // Parent topic references used to build crumbs.
        mockTopicIndex
            .Setup(mock => mock.GetParentTopic(
                It.Is<string>(topicPath => topicPath == "/guidance/example-topic")
            ))
            .Returns(GuidanceSectionTopic);

        mockTopicIndex
            .Setup(mock => mock.GetParentTopic(
                It.Is<string>(topicPath => topicPath == "/guidance")
            ))
            .Returns(HomePageTopic);

        mockTopicIndex
            .Setup(mock => mock.GetParentTopic(
                It.Is<string>(topicPath => topicPath == "/")
            ))
            .Returns<TopicModel>(null!);

        var controller = CreateController(autoMocker, "/guidance/example-topic");

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<TopicViewModel>(result);

        Assert.HasCount(2, viewModel.Crumbs);
        Assert.AreEqual("Home", viewModel.Crumbs.ElementAt(0).Text);
        Assert.AreEqual("/", viewModel.Crumbs.ElementAt(0).Href.ToString());
        Assert.AreEqual("Guidance", viewModel.Crumbs.ElementAt(1).Text);
        Assert.AreEqual("/guidance", viewModel.Crumbs.ElementAt(1).Href.ToString());
    }

    [TestMethod]
    public async Task Index_SkipsRootCrumb_WhenIsGlobal()
    {
        var autoMocker = new AutoMocker();
        var mockTopicIndex = SetupTopicIndex(autoMocker);

        // Parent topic references used to build crumbs.
        mockTopicIndex
            .Setup(mock => mock.GetParentTopic(
                It.Is<string>(topicPath => topicPath == "/cookies")
            ))
            .Returns(HomePageTopic);

        mockTopicIndex
            .Setup(mock => mock.GetParentTopic(
                It.Is<string>(topicPath => topicPath == "/")
            ))
            .Returns<TopicModel>(null!);

        var controller = CreateController(autoMocker, "/cookies");

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<TopicViewModel>(result);

        Assert.HasCount(0, viewModel.Crumbs);
    }

    [TestMethod]
    public async Task Index_PresentsAsHelpTopic_WhenIsGlobalIsFalse()
    {
        var autoMocker = new AutoMocker();
        SetupTopicIndex(autoMocker);

        var controller = CreateController(autoMocker, "/guidance");

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<TopicViewModel>(result);

        Assert.IsFalse(viewModel.IsGlobal);
    }

    [TestMethod]
    public async Task Index_PresentsAsGlobalTopic_WhenIsGlobalIsTrue()
    {
        var autoMocker = new AutoMocker();
        SetupTopicIndex(autoMocker);

        var controller = CreateController(autoMocker, "/cookies");

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<TopicViewModel>(result);

        Assert.IsTrue(viewModel.IsGlobal);
    }

    [TestMethod]
    public async Task Index_PresentsTopicListing_WhenDefined()
    {
        var autoMocker = new AutoMocker();
        SetupTopicIndex(autoMocker);

        var controller = CreateController(autoMocker, "/guidance");

        var result = await controller.Index();

        var viewModel = TypeAssert.IsViewModelType<TopicViewModel>(result);

        Assert.HasCount(1, viewModel.TopicListingSections);

        var resultTopicListing = viewModel.TopicListingSections.First();
        Assert.AreEqual("Topics in this section", resultTopicListing.Heading);
        Assert.HasCount(1, resultTopicListing.Topics);

        var resultTopic = resultTopicListing.Topics.First();
        Assert.AreEqual("/guidance/example-topic", resultTopic.Href.ToString());
        Assert.AreEqual("Example", resultTopic.Title);
        Assert.AreEqual("Summary of example topic.", resultTopic.Summary);
    }
}
