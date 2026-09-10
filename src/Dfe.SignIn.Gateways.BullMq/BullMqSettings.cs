namespace Dfe.SignIn.Gateways.BullMq;

/// <summary>
/// Represents the settings for configuring BullMQ.
/// </summary>
public sealed class BullMqSettings
{
    /// <summary>
    /// Gets the name of the configuration section that contains the BullMQ settings.
    /// </summary>
    public const string SectionName = "BullMq";

    /// <summary>
    /// Gets or sets the Redis connection string used by BullMQ.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the Redis database index applied to the connection string during service registration.
    /// </summary>
    public required int DatabaseIndex { get; set; } = 4;

    /// <summary>
    /// Gets or sets how long (in seconds) completed jobs are retained before removal.
    /// </summary>
    public required int RemoveOnCompleteAgeSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets or sets the maximum number of completed jobs to retain.
    /// </summary>
    public required int RemoveOnCompleteCount { get; set; } = 50;

    /// <summary>
    /// Gets or sets how long (in seconds) failed jobs are retained before removal.
    /// </summary>
    public required int RemoveOnFailAgeSeconds { get; set; } = 12 * 3600;
}
