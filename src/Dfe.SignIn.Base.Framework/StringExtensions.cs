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

    /// <summary>
    /// Converts a string to a Guid, throwing a FormatException if the format is invalid.
    /// </summary>
    /// <param name="input">The string to convert to a Guid.</param>
    /// <returns>The Guid representation of the string.</returns>
    /// <exception cref="FormatException">Thrown when the string is not a valid Guid format.</exception>
    public static Guid ToGuid(this string input)
        => Guid.TryParse(input, out var result) ? result : throw new FormatException($"Invalid GUID format: '{input}'");
}
