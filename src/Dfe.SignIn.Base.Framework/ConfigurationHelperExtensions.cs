using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Extensions to help with configuring applications.
/// </summary>
public static class ConfigurationHelperExtensions
{
    /// <summary>
    /// Gets JSON encoded data from configuration section.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="section">Configuration section.</param>
    /// <param name="key">Key of configuration entry.</param>
    /// <returns>
    ///   <para>The JSON encoded data when present; otherwise, a value null.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="section"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="key"/> is null or empty.</para>
    /// </exception>
    public static T? GetJson<T>(this IConfiguration section, string key)
        where T : class
    {
        ExceptionHelpers.ThrowIfArgumentNull(section, nameof(section));
        ExceptionHelpers.ThrowIfArgumentNullOrEmpty(key, nameof(key));

        string? json = section[key];
        return !string.IsNullOrWhiteSpace(json)
            ? JsonSerializer.Deserialize<T>(json)
            : section.GetSection(key)?.Get<T>();
    }

    /// <summary>
    /// Gets JSON encoded list of elements from configuration section.
    /// </summary>
    /// <typeparam name="T">The type of element.</typeparam>
    /// <param name="section">Configuration section.</param>
    /// <param name="key">Key of configuration entry.</param>
    /// <returns>
    ///   <para>The JSON encoded list of elements when present; otherwise, an empty list.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="section"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="key"/> is null or empty.</para>
    /// </exception>
    public static List<T> GetJsonList<T>(this IConfiguration section, string key)
    {
        return GetJson<List<T>>(section, key) ?? [];
    }

    /// <summary>
    /// Gets JSON encoded list of strings from configuration section.
    /// </summary>
    /// <param name="section">Configuration section.</param>
    /// <param name="key">Key of configuration entry.</param>
    /// <returns>
    ///   <para>The JSON encoded list of strings when present; otherwise, an empty list.</para>
    /// </returns>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="section"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="key"/> is null or empty.</para>
    /// </exception>
    public static List<string> GetJsonList(this IConfiguration section, string key)
    {
        return GetJsonList<string>(section, key) ?? [];
    }
}
