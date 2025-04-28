using System.Text;
using System.Text.Json;
using Dfe.SignIn.DocfxPlugin.Helpers;

namespace Dfe.SignIn.DocfxPlugin.Models;

internal class TocEntry
{
    public string? Name { get; set; }

    public string? Href { get; set; }

    public string? TocHref { get; set; }

    public string? Type { get; set; }

    public string? MemberLayout { get; set; }

    public TocEntry[]? Items { get; set; }

    #region Entries with crumbs

    public TocEntryResult? FindEntryWithCrumbs(string href)
    {
        var crumbs = new List<TocEntry>();
        if (this.FindEntryWithCrumbs(crumbs, href)) {
            crumbs.Reverse();
            return new() {
                Entry = crumbs.Last(),
                Crumbs = [.. crumbs],
            };
        }
        return null;
    }

    private bool FindEntryWithCrumbs(List<TocEntry> crumbs, string href)
    {
        if (href == this.Href) {
            crumbs.Add(this);
            return true;
        }
        foreach (var item in this.Items ?? []) {
            if (item.FindEntryWithCrumbs(crumbs, href)) {
                crumbs.Add(this);
                return true;
            }
        }
        return false;
    }

    #endregion
}

internal sealed class Toc : TocEntry
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static Toc FromFile(string outputPath, string filePath)
    {
        string json = File.ReadAllText(filePath, Encoding.UTF8);
        var toc = JsonSerializer.Deserialize<Toc>(json, JsonOptions)!;
        toc.FilePath = filePath;
        string basePath = PathHelpers.GetRelativePath(outputPath, Path.GetDirectoryName(filePath)!);
        TransformEntry(toc, basePath);
        return toc;
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

    public string FilePath { get; private set; } = "";
}
