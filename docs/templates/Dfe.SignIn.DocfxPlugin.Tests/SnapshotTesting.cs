using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Dfe.SignIn.DocfxPlugin.Tests;

[TestClass]
[DoNotParallelize]
public sealed class SnapshotTesting : VerifyBase
{
    private static readonly VerifySettings Settings;

    static SnapshotTesting()
    {
        Settings = new VerifySettings();
        Settings.UseDirectory("snapshots");
    }

    [TestMethod]
    public async Task BuildsSampleDocumentation()
    {
        string sampleProjectPath = Path.GetFullPath("../../../../Dfe.SignIn.DocfxPlugin.Tests/sample-docs/docfx.json");
        using var process = Process.Start("dotnet", $"docfx \"{sampleProjectPath}\"");
        await process.WaitForExitAsync();
        VerifyExitCode(process);
    }

    private static string OutputPath => Path.GetFullPath("../../../../Dfe.SignIn.DocfxPlugin.Tests/sample-docs/_site");

    private static IEnumerable<string> GetOutputFilePaths()
    {
        return [
            Path.Join(OutputPath, "search.json"),
            .. DiscoverOutputHtmlFiles(OutputPath),
        ];
    }

    private static IEnumerable<string> DiscoverOutputHtmlFiles(string outputPath)
    {
        var matcher = new Matcher();
        matcher.AddInclude("**.html");
        return matcher.GetResultsInFullPath(outputPath);
    }

    [DynamicData(nameof(GetOutputFilePaths), DynamicDataSourceType.Method)]
    [TestMethod]
    public async Task VerifyOutputFile(string filePath)
    {
        string html = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        string testFileName = Path.GetRelativePath(OutputPath, filePath).Replace('\\', '_').Replace('/', '_');
        await this.Verify(html, Settings).UseFileName(testFileName);
    }

    private static void VerifyExitCode(Process process)
    {
        if (!process.HasExited) {
            throw new InvalidOperationException("Process is not exited yet.");
        }
        // Gets exit code before closing process.
        var exitCode = process.ExitCode;
        // Close process to flush stdout/stderr logs.
        process.Close();
        // Assert exit code
        Assert.AreEqual(0, exitCode);
    }
}
