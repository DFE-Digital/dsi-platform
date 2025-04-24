using System.Text;
using System.Text.Json;
using Dfe.SignIn.DocfxPlugin.Helpers;

namespace Dfe.SignIn.DocfxPlugin.Models;

internal sealed class TocEntry
{
    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true,
    };

    public static TocEntry FromFile(string outputPath, string filePath)
    {
        string json = File.ReadAllText(filePath, Encoding.UTF8);
        var tocRoot = JsonSerializer.Deserialize<TocEntry>(json, JsonOptions)!;
        string basePath = PathHelpers.GetRelativePath(outputPath, Path.GetDirectoryName(filePath)!);
        TransformEntry(tocRoot, basePath);
        return tocRoot;
    }

    private static void TransformEntry(TocEntry entry, string basePath)
    {
        if (entry.Href is not null) {
            entry.Href = PathHelpers.ResolveRelativePath(basePath, entry.Href);
        }
        foreach (var childEntry in entry.Items ?? []) {
            TransformEntry(childEntry, basePath);
        }
    }

    public string? Name { get; set; }

    public string? Href { get; set; }

    public string? Type { get; set; }

    public string? MemberLayout { get; set; }

    public TocEntry[]? Items { get; set; }

    public TocEntryResult? FindEntry(string href)
    {
        var crumbs = new List<TocEntry>();
        if (this.FindEntry(crumbs, href)) {
            crumbs.Reverse();
            return new() {
                Entry = crumbs.Last(),
                Crumbs = [.. crumbs],
            };
        }
        return null;
    }

    private bool FindEntry(List<TocEntry> crumbs, string href)
    {
        if (href == this.Href) {
            crumbs.Add(this);
            return true;
        }
        foreach (var item in this.Items ?? []) {
            if (item.FindEntry(crumbs, href)) {
                crumbs.Add(this);
                return true;
            }
        }
        return false;
    }
}
