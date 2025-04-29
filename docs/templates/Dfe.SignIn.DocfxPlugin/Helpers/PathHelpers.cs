namespace Dfe.SignIn.DocfxPlugin.Helpers;

internal static class PathHelpers
{
    public static string Normalize(string path)
    {
        return path.Replace("\\", "/");
    }

    public static string GetRelativePath(string relativeTo, string path)
    {
        return Normalize(Path.GetRelativePath(relativeTo, path));
    }

    public static string ResolveRelativePath(string basePath, string relativePath)
    {
        basePath = basePath.Trim('/', '\\');
        if (basePath == ".") {
            return relativePath;
        }
        if (relativePath.StartsWith('/') || relativePath.StartsWith("http:") || relativePath.StartsWith("https:")) {
            return relativePath;
        }
        return basePath + "/" + relativePath;
    }
}
