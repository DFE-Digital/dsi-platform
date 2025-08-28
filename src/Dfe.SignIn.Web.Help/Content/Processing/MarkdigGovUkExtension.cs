using Markdig;
using Markdig.Extensions.Tables;
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
        pipeline.DocumentProcessed += ProcessDocument;
    }

    /// <inheritdoc/>
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }

    private static void ProcessDocument(MarkdownDocument document)
    {
        foreach (var block in document.Descendants()) {
            if (block is HeadingBlock headingBlock) {
                ProcessHeadingBlock(headingBlock);
            }
            else if (block is ParagraphBlock paragraphBlock) {
                ProcessParagraphBlock(paragraphBlock);
            }
            else if (block is ListBlock listBlock) {
                ProcessListBlock(listBlock);
            }
            else if (block is Table tableBlock) {
                ProcessTableBlock(tableBlock);
            }
            else if (block is LinkInline linkInline) {
                ProcessLinkInline(linkInline);
            }
        }
    }

    private static void ProcessHeadingBlock(HeadingBlock block)
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

    private static void ProcessParagraphBlock(ParagraphBlock block)
    {
        var attributes = block.GetAttributes();
        attributes.AddClass("govuk-body");
    }

    private static void ProcessListBlock(ListBlock block)
    {
        var attributes = block.GetAttributes();
        attributes.AddClass("govuk-list");
        attributes.AddClass(
            block.IsOrdered
                ? "govuk-list--number"
                : "govuk-list--bullet"
        );
    }

    private static void ProcessTableBlock(Table block)
    {
        var attributes = block.GetAttributes();
        attributes.AddClass("govuk-table");

        var rows = block.Descendants<TableRow>();
        AddClassName(rows, "govuk-table__row");
        var headerCells = rows.Where(row => row.IsHeader).SelectMany(row => row.Descendants<TableCell>());
        AddClassName(headerCells, "govuk-table__header");
        var normalCells = rows.Where(row => !row.IsHeader).SelectMany(row => row.Descendants<TableCell>());
        AddClassName(normalCells, "govuk-table__cell");
    }

    private static void ProcessLinkInline(LinkInline block)
    {
        var attributes = block.GetAttributes();
        attributes.AddClass("govuk-link");
    }

    private static void AddClassName(IEnumerable<Block> blocks, string name)
    {
        foreach (var block in blocks) {
            block.GetAttributes().AddClass(name);
        }
    }
}
