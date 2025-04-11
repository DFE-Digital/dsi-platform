#if NET6_0_OR_GREATER
using System.Diagnostics;
#endif

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Provides helper methods for common exception validation.
/// </summary>
public static class ExceptionHelpers
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified value is null.
    /// </summary>
    /// <param name="value">The object to check for null.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="value"/> is null.</para>
    /// </exception>
#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif
    public static void ThrowIfArgumentNull(object value, string paramName)
    {
        if (value is null) {
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified string is null,
    /// and an <see cref="ArgumentException"/> if the string is empty.
    /// </summary>
    /// <param name="value">The string to check for null or empty.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="value"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="value"/> is an empty string.</para>
    /// </exception>
#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif
    public static void ThrowIfArgumentNullOrEmpty(string value, string paramName)
    {
        if (value is null) {
            throw new ArgumentNullException(paramName);
        }
        if (string.IsNullOrEmpty(value)) {
            throw new ArgumentException("Empty string.", paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified string is null,
    /// and an <see cref="ArgumentException"/> if the string consists only of whitespace.
    /// </summary>
    /// <param name="value">The string to check for null or whitespace.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="value"/> is null.</para>
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="value"/> consists only of whitespace.</para>
    /// </exception>
#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif
    public static void ThrowIfArgumentNullOrWhiteSpace(string value, string paramName)
    {
        if (value is null) {
            throw new ArgumentNullException(paramName);
        }
        if (string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException("White space.", paramName);
        }
    }
}
