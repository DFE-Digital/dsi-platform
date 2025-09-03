using Dfe.SignIn.Base.Framework;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Renderers;
using Markdig.Syntax;
using YamlDotNet.Serialization;

namespace Dfe.SignIn.Web.Help.Content.Processing;

/// <summary>
/// A concrete implementation of <see cref="ITopicMarkdownProcessor"/>.
/// </summary>
public partial class MarkdigTopicMarkdownProcessor : ITopicMarkdownProcessor
{
    private readonly MarkdownPipeline pipeline;
    private readonly IDeserializer frontmatterDeserializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicMarkdownProcessor"/> class.
    /// </summary>
    public MarkdigTopicMarkdownProcessor()
    {
        this.pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .UsePipeTables()
            .Use<MarkdigGovUkExtension>()
            .Build();

        this.frontmatterDeserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithCaseInsensitivePropertyMatching()
            .Build();
    }

    /// <inheritdoc/>
    public Task<TopicModel> ProcessMarkdownAsync(string topicPath, string markdown, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNullOrWhiteSpace(topicPath, nameof(topicPath));
        ExceptionHelpers.ThrowIfArgumentNull(markdown, nameof(markdown));

        var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        this.pipeline.Setup(renderer);

        var document = Markdown.Parse(markdown, this.pipeline);
        var frontmatter = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        string frontmatterYaml = string.Join("\n", frontmatter?.Lines);

        var metadata = this.frontmatterDeserializer.Deserialize<TopicMetadata>(frontmatterYaml)
            ?? Activator.CreateInstance<TopicMetadata>();

        if (string.IsNullOrEmpty(metadata.Title)) {
            metadata = metadata with {
                Title = TopicPathHelpers.TopicPathToTitle(topicPath),
            };
        }
        if (string.IsNullOrEmpty(metadata.NavigationTitle)) {
            metadata = metadata with {
                NavigationTitle = metadata.Title,
            };
        }

        renderer.Render(document);
        writer.Flush();

        return Task.FromResult(new TopicModel {
            Path = topicPath,
            Metadata = metadata,
            ContentHtml = writer.ToString(),
        });
    }
}
