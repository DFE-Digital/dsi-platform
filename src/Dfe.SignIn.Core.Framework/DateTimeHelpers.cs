namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Provides helper methods and constants for working with DateTime values.
/// </summary>
public static class DateTimeHelpers
{
    /// <summary>
    /// Represents the Unix epoch, which is the starting point for Unix time
    /// (January 1, 1970, 00:00:00 UTC).
    /// </summary>
    public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
