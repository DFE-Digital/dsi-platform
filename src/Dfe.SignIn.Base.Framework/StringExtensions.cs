namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Common helper functionality for strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Trims whitespace from start and end of strings. Replaces many spaces between words with one.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>cleansed string</returns>
    public static string NormalizeWhitespace(this string input)
        => string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : string.Join(" ",
                input.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries));
}
