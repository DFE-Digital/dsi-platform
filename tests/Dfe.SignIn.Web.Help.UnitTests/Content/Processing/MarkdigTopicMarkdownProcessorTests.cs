using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Content.Processing;
using HtmlAgilityPack;

namespace Dfe.SignIn.Web.Help.UnitTests.Content.Processing;

[TestClass]
public sealed class MarkdigTopicMarkdownProcessorTests
{
    private readonly MarkdigTopicMarkdownProcessor processor = new();

    [TestMethod]
    public Task ProcessMarkdownAsync_Throws_WhenTopicPathArgumentIsNull()
    {
        return Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => this.processor.ProcessMarkdownAsync(null!, ""));
    }

    [TestMethod]
    public Task ProcessMarkdownAsync_Throws_WhenTopicPathArgumentIsEmptyString()
    {
        return Assert.ThrowsExactlyAsync<ArgumentException>(()
            => this.processor.ProcessMarkdownAsync("", ""));
    }

    [TestMethod]
    public Task ProcessMarkdownAsync_Throws_WhenMarkdownArgumentIsNull()
    {
        return Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => this.processor.ProcessMarkdownAsync("/", null!));
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsTopicPath()
    {
        string markdown = """
        Example content.
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/my-account/change-password", markdown);

        Assert.AreEqual("/my-account/change-password", topic.Path);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsContentHtmlFromRenderedMarkup()
    {
        string markdown = """
        Example content.
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/my-account/change-password", markdown);

        var doc = new HtmlDocument();
        doc.LoadHtml(topic.ContentHtml);

        var firstParagraph = doc.DocumentNode.Descendants("p").FirstOrDefault();
        Assert.IsNotNull(firstParagraph);
        Assert.IsTrue(firstParagraph.HasClass("govuk-body"));
        Assert.AreEqual("Example content.", firstParagraph.InnerText);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_CanRenderPipeTables()
    {
        string markdown = """
        | A   | B   |
        | --- | --- |
        | 1   | 2   |
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/", markdown);

        var doc = new HtmlDocument();
        doc.LoadHtml(topic.ContentHtml);

        var table = doc.DocumentNode.Descendants("table").FirstOrDefault();
        Assert.IsNotNull(table);
        Assert.IsTrue(table.HasClass("govuk-table"));
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsTitleAsSpecified()
    {
        string markdown = """
        ---
        title: Example title
        ---
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/", markdown);

        Assert.AreEqual("Example title", topic.Metadata.Title);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsTitleFromTopicPath_WhenTitleNotSpecified()
    {
        string markdown = """
        Example content
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/example-article", markdown);

        Assert.AreEqual("Example article", topic.Metadata.Title);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsDefaultTitle_WhenTitleCannotBeInferredFromTopicPath()
    {
        string markdown = """
        Example content
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/", markdown);

        Assert.AreEqual(TopicPathHelpers.DefaultTitle, topic.Metadata.Title);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsNavigationTitleAsSpecified()
    {
        string markdown = """
        ---
        title: Example title
        navigationTitle: Example
        ---
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/", markdown);

        Assert.AreEqual("Example", topic.Metadata.NavigationTitle);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsNavigationTitleFromTitle_WhenNavigationTitleNotSpecified()
    {
        string markdown = """
        ---
        title: Example title
        ---
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/", markdown);

        Assert.AreEqual("Example title", topic.Metadata.NavigationTitle);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsNavigationTitleFromTopicPath_WhenNeitherTitleWasSpecified()
    {
        string markdown = """
        Example content
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/example-article", markdown);

        Assert.AreEqual("Example article", topic.Metadata.NavigationTitle);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsSummaryAsSpecified()
    {
        string markdown = """
        ---
        title: Example title
        summary: |
            Example summary text.
        ---
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/", markdown);

        Assert.AreEqual("Example summary text.", topic.Metadata.Summary);
    }

    [TestMethod]
    public async Task ProcessMarkdownAsync_SetsTopicsAsSpecified()
    {
        string markdown = """
        ---
        title: Example title
        topics:
          - heading: First section
            paths:
              - /my-account/changing-my-password
              - /my-account/multifactor-authentication

          - paths:
              - /my-service/managing-my-service
        ---
        """;

        var topic = await this.processor.ProcessMarkdownAsync("/", markdown);

        var sections = topic.Metadata.Topics.ToArray();
        Assert.AreEqual(2, sections.Length);

        Assert.AreEqual("First section", sections[0].Heading);
        string[] firstSectionExpectedPaths = [
            "/my-account/changing-my-password",
            "/my-account/multifactor-authentication",
        ];
        CollectionAssert.AreEqual(firstSectionExpectedPaths, sections[0].Paths.ToArray());

        Assert.AreEqual("Topics in this section", sections[1].Heading);
        string[] secondSectionExpectedPaths = [
            "/my-service/managing-my-service",
        ];
        CollectionAssert.AreEqual(secondSectionExpectedPaths, sections[1].Paths.ToArray());
    }
}
