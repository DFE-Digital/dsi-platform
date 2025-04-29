namespace Dfe.SignIn.DocfxPlugin.Models;

internal sealed class IndexEntry
{
    public int Id { get; set; }

    public required string Href { get; set; }

    public required string Title { get; set; }

    public required string Summary { get; set; }

    public string? Section { get; set; }
}
