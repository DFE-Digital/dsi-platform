using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Dfe.SignIn.DocfxPlugin.Models;

internal sealed class TocMap
{
    public static TocMap FromDirectory(string outputPath)
    {
        var map = new TocMap();

        var matcher = new Matcher();
        matcher.AddInclude("**/toc.json");
        var matchingFiles = matcher.GetResultsInFullPath(outputPath);

        foreach (string file in matchingFiles) {
            var toc = Toc.FromFile(outputPath, file);
            string key = Path.GetRelativePath(outputPath, Path.GetDirectoryName(file)!);
            map.mappings.Add(key, toc);
        }

        return map;
    }

    private readonly Dictionary<string, Toc> mappings = [];

    public bool TryGetToc(string? path, [MaybeNullWhen(false)] out Toc result)
    {
        if (path is null) {
            result = null;
            return false;
        }
        string key = Path.GetDirectoryName(path) ?? ".";
        if (this.mappings.TryGetValue(key, out result)) {
            return true;
        }
        return key != "." && this.TryGetToc(key, out result);
    }
}
