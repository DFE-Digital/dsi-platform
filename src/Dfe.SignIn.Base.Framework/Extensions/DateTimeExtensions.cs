namespace Dfe.SignIn.Base.Framework.Extensions;

/// <summary>
/// Extensions for DateTime to handle conversion to UTC, treating unspecified kinds as UTC by default.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Returns the UTC equivalent of the given DateTime. If the input DateTime has an
    /// unspecified kind, it will be treated as UTC.
    /// </summary>
    /// <param name="date">The DateTime to convert to UTC.</param>
    /// <returns>The UTC equivalent of the given DateTime.</returns>
    public static DateTime ToUtc(this DateTime date)
        => date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime();

    /// <summary>
    /// Returns the UTC equivalent of the given DateTime, or null if the input is null.
    /// </summary>
    /// <param name="date">The nullable DateTime to convert to UTC.</param>
    /// <returns>The UTC equivalent of the given DateTime, or null if the input is null.</returns>
    public static DateTime? ToUtc(this DateTime? date)
        => date.HasValue ? ToUtc(date.Value) : null;
}
