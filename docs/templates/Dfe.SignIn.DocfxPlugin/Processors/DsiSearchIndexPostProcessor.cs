using System.Text;
using System.Text.Json;
using Dfe.SignIn.DocfxPlugin.Models;
using Docfx.Plugins;
using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;

namespace Dfe.SignIn.DocfxPlugin.Processors;

/// <summary>
/// A custom post-processor plugin for DocFX which tidies up the output search index.
/// </summary>
[Export(nameof(DsiSearchIndexPostProcessor), typeof(IPostProcessor))]
public sealed partial class DsiSearchIndexPostProcessor : IPostProcessor
{
    /// <inheritdoc/>
    public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
    {
        return metadata;
    }

    /// <inheritdoc/>
    public Manifest Process(Manifest manifest, string outputFolder, CancellationToken cancellationToken)
    {
        Task.WaitAll([
            PostProcessSearchIndexFile(outputFolder, cancellationToken),
        ], cancellationToken);

        return manifest;
    }

    [GeneratedRegex("^part|method|property|field|event|constructor$", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex DeterminePrepositionSymbolPattern();

    private static async Task PostProcessSearchIndexFile(string outputFolder, CancellationToken cancellationToken)
    {
        var tocs = TocMap.FromDirectory(outputFolder);

        string inputFilePath = Path.Join(outputFolder, "index.json");
        string indexJson = await File.ReadAllTextAsync(inputFilePath, Encoding.UTF8, cancellationToken);
        var index = JsonSerializer.Deserialize<Dictionary<string, IndexEntry>>(indexJson, JsonSerializerOptions.Web)!;

        var values = index.Values.ToArray();

        for (int i = 0; i < values.Length; ++i) {
            var entry = values[i];

            entry.Id = i;
            entry.Title = entry.Title.Split("|")[0].Trim();

            if (tocs.TryGetToc(entry.Href, out var toc)) {
                string? sectionName = toc.Name;

                var tocEntry = toc.FindEntryWithCrumbs(entry.Href);
                if (tocEntry is not null) {
                    if (!string.IsNullOrWhiteSpace(tocEntry.Entry.Name)) {
                        entry.Title = tocEntry.Entry.Name;
                    }
                    if (tocEntry.Crumbs.Length > 2) {
                        sectionName = tocEntry.Crumbs[^3].Name;
                    }
                    else if (tocEntry.Crumbs.Length > 1) {
                        sectionName = tocEntry.Crumbs[^2].Name;
                    }
                }

                string type = !string.IsNullOrWhiteSpace(tocEntry?.Entry.Type) ? tocEntry.Entry.Type : "part";
                entry.Section = $"{type.ToLower()} {(DeterminePrepositionSymbolPattern().IsMatch(type) ? "of" : "in")} {sectionName ?? "Reference"}";
            }
        }

        indexJson = JsonSerializer.Serialize(values, JsonSerializerOptions.Web);
        string outputFilePath = Path.Join(outputFolder, "search.json");
        await File.WriteAllTextAsync(outputFilePath, indexJson, Encoding.UTF8, cancellationToken);
    }
}
