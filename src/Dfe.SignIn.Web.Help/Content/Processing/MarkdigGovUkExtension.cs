using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Dfe.SignIn.Web.Help.Content.Processing;

/// <summary>
/// A custom extension for Markdig which adds GDS classes to rendered HTML elements.
/// </summary>
/// <remarks>
///   <para>As an example, the "govuk-body" class is added to paragraph elements.</para>
/// </remarks>
public sealed class MarkdigGovUkExtension : IMarkdownExtension
{
    /// <inheritdoc/>
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.DocumentProcessed += this.ProcessDocument;
    }

    /// <inheritdoc/>
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }

    private void ProcessDocument(MarkdownDocument document)
    {
        foreach (var block in document.Descendants()) {
            if (block is HeadingBlock headingBlock) {
                this.ProcessHeadingBlock(headingBlock);
            }
            else if (block is ParagraphBlock paragraphBlock) {
                this.ProcessParagraphBlock(paragraphBlock);
            }
            else if (block is ListBlock listBlock) {
                this.ProcessListBlock(listBlock);
            }
            else if (block is LinkInline linkInline) {
                this.ProcessLinkInline(linkInline);
            }
        }
    }

    private void ProcessHeadingBlock(HeadingBlock block)
    {
        var attributes = block.GetAttributes();
        switch (block.Level) {
            case 1:
                attributes.AddClass("govuk-heading-l");
                break;
            case 2:
                attributes.AddClass("govuk-heading-m");
                break;
            case 3:
                attributes.AddClass("govuk-heading-s");
                break;
            default:
                attributes.AddClass("govuk-heading-xs");
                break;
        }
    }

    private void ProcessParagraphBlock(ParagraphBlock block)
    {
        var attributes = block.GetAttributes();
        attributes.AddClass("govuk-body");
    }

    private void ProcessListBlock(ListBlock block)
    {
        var attributes = block.GetAttributes();
        attributes.AddClass("govuk-list");
        attributes.AddClass(
            block.IsOrdered
                ? "govuk-list--number"
                : "govuk-list--bullet"
        );
    }

    private void ProcessLinkInline(LinkInline block)
    {
        var attributes = block.GetAttributes();
        attributes.AddClass("govuk-link");
    }
}
