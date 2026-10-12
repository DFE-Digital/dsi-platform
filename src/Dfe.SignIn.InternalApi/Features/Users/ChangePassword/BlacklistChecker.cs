
namespace Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

/// <summary>
/// A utility class for determining if a provided
/// password is on the blacklist
/// </summary>
public static class PasswordChecker
{
    //Hashset used for speed
    private static readonly HashSet<string> passwords = new(StringComparer.Ordinal);

    /// <summary>
    /// Loads the passwords into a hashset
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Initialize()
    {
        var resourceNamespace = typeof(PasswordChecker).Assembly.GetName().Name;
        AddToBlacklist($"{resourceNamespace}.dfeBlacklist.txt");
        AddToBlacklist($"{resourceNamespace}.ncscBlacklist.txt");
    }

    private static void AddToBlacklist(string resourceName)
    {
        using var stream = typeof(PasswordChecker).Assembly
                    .GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException("Resource not found.");

        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line) {
            passwords.Add(line);
        }
    }

    /// <summary>
    /// Checks if the passed in parameter is on the blacklist
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public static bool IsBlacklisted(string password)
    {
        return passwords.Contains(password);
    }
}
