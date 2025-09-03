using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.PublicApi.Client.Abstractions;

/// <summary>
/// Extension methods for the <see cref="IHttpRequest"/> abstraction.
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// Gets a required named value from the query string.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="key">Name of the value.</param>
    /// <returns>
    ///   <para>The value when present; otherwise, a value of <c>null</c>.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="request"/> is null.</para>
    ///   <para>- or -</para>
    ///   <para>If <paramref name="key"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="key"/> is an empty string.</para>
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///   <para>If there was no query parameter for the given key.</para>
    /// </exception>
    public static string GetRequiredQuery(this IHttpRequest request, string key)
    {
        ExceptionHelpers.ThrowIfArgumentNull(request, nameof(request));

        return request.GetQuery(key)
            ?? throw new KeyNotFoundException($"Missing required parameter '{key}'.");
    }
}
