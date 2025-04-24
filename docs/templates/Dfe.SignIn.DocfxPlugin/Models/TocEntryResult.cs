namespace Dfe.SignIn.DocfxPlugin.Models;

internal sealed record TocEntryResult
{
    public required TocEntry Entry { get; init; }

    public required TocEntry[] Crumbs { get; init; }
}
