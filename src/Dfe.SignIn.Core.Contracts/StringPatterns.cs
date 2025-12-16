using System.Reflection;
using System.Text.RegularExpressions;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts;

/// <summary>
/// Common regular expression patterns for validating strings.
/// </summary>
public static partial class StringPatterns
{
    private static Dictionary<string, string>? exampleValues;

    private static Dictionary<string, string> DiscoverExampleValues()
    {
        return typeof(StringPatterns)
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .Select(field => (field, example: field.GetCustomAttribute<ExampleValueAttribute>()))
            .Where(entry => entry.example is not null)
            .ToDictionary(
                entry => entry.field.GetRawConstantValue()!.ToString()!,
                entry => entry.example!.Value
            );
    }

    /// <summary>
    /// Gets an example value for the given pattern.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <returns>
    ///   <para>An example value where possible; otherwise, a value of null.</para>
    /// </returns>
    public static string? GetExampleValue(string? pattern)
    {
        if (pattern is null) {
            return null;
        }

        exampleValues ??= DiscoverExampleValues();
        exampleValues.TryGetValue(pattern, out var value);
        return value;
    }

    private const string NamePattern = @"[\p{L}\p{N}_][\p{L}\p{N}_'-]*( [\p{L}\p{N}][\p{L}\p{N} _'-]*)*";

    /// <summary>
    /// Regular expression pattern which can be used to verify the first name of a person.
    /// </summary>
    [ExampleValue("Alex")]
    public const string FirstNamePattern = $"^(?<first>{NamePattern})$";

    [GeneratedRegex(FirstNamePattern)]
    public static partial Regex FirstNameRegex();

    /// <summary>
    /// Regular expression pattern which can be used to verify the last name of a person.
    /// </summary>
    [ExampleValue("Johnson")]
    public const string LastNamePattern = $"^(?<last>{NamePattern})$";

    [GeneratedRegex(LastNamePattern)]
    public static partial Regex LastNameRegex();

    /// <summary>
    /// Regular expression pattern which can be used to verify the full name of a person.
    /// </summary>
    [ExampleValue("Alex Johnson")]
    public const string FullNamePattern = $"^(?<full>{NamePattern})$";

    [GeneratedRegex(FullNamePattern)]
    public static partial Regex FullNameRegex();

    /// <summary>
    /// Regular expression pattern which can be used to verify an email address.
    /// </summary>
    /// <remarks>
    ///   <para>Whilst this expression does not accurately represent the RFC standard;
    ///   it should catch obvious errors in user input.</para>
    /// </remarks>
    [ExampleValue("alex.johnson@example.com")]
    public const string EmailAddressPattern
        = @"^[\p{L}\p{N}_']([\p{L}\p{N}_'+.-]*[\p{L}\p{N}_'])?@[\p{L}\p{N}_-]+(\.[\p{L}\p{N}_-]*[\p{L}\p{N}])+$";

    [GeneratedRegex(EmailAddressPattern)]
    public static partial Regex EmailAddressRegex();

    /// <summary>
    /// Regular expression pattern which can be used to verify the job title of a person.
    /// </summary>
    [ExampleValue("Software Developer")]
    public const string JobTitlePattern
        = @"^([\p{L}\p{N}()]+( [\p{L}\p{N}()]+)*)?$";

    [GeneratedRegex(JobTitlePattern)]
    public static partial Regex JobTitleRegex();
}
