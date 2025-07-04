using Dfe.SignIn.Web.Help.Content.Processing;
using HtmlAgilityPack;
using Markdig;

namespace Dfe.SignIn.Web.Help.UnitTests.Content.Processing;

[TestClass]
public sealed class MarkdigGovUkExtensionTests
{
    private MarkdownPipeline markdownPipeline = null!;

    [TestInitialize]
    public void Initialize()
    {
        this.markdownPipeline = new MarkdownPipelineBuilder()
            .Use<MarkdigGovUkExtension>()
            .Build();
    }

    private HtmlDocument ParseMarkdown(string markdown)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(Markdown.ToHtml(markdown, this.markdownPipeline));
        return doc;
    }

    [DataRow("# heading", "h1", "govuk-heading-l")]
    [DataRow("## heading", "h2", "govuk-heading-m")]
    [DataRow("### heading", "h3", "govuk-heading-s")]
    [DataRow("#### heading", "h4", "govuk-heading-xs")]
    [DataTestMethod]
    public void DocumentProcessed_Heading_HasExpectedClass(string markdown, string elementName, string expectedClass)
    {
        var doc = this.ParseMarkdown(markdown);
        var element = doc.DocumentNode.Descendants(elementName).FirstOrDefault();

        Assert.IsNotNull(element);
        Assert.IsTrue(element.HasClass(expectedClass));
    }

    [TestMethod]
    public void DocumentProcessed_Paragraph_HasExpectedClass()
    {
        var doc = this.ParseMarkdown("Paragraph of text.");
        var element = doc.DocumentNode.Descendants("p").FirstOrDefault();

        Assert.IsNotNull(element);
        Assert.IsTrue(element.HasClass("govuk-body"));
    }

    [DataRow(
        """
        - First item
        - Second item
        """,
        "ul",
        "govuk-list--bullet"
    )]
    [DataRow(
        """
        1. First item
        2. Second item
        """,
        "ol",
        "govuk-list--number"
    )]
    [DataTestMethod]
    public void DocumentProcessed_List_HasExpectedClass(string markdown, string elementName, string expectedClass)
    {
        var doc = this.ParseMarkdown(markdown);
        var element = doc.DocumentNode.Descendants(elementName).FirstOrDefault();

        Assert.IsNotNull(element);
        Assert.IsTrue(element.HasClass("govuk-list"));
        Assert.IsTrue(element.HasClass(expectedClass));
    }

    [TestMethod]
    public void DocumentProcessed_Hyperlink_HasExpectedClass()
    {
        var doc = this.ParseMarkdown("Paragraph of text with [link](https://example.localhost).");
        var element = doc.DocumentNode.Descendants("a").FirstOrDefault();

        Assert.IsNotNull(element);
        Assert.IsTrue(element.HasClass("govuk-link"));
    }
}
