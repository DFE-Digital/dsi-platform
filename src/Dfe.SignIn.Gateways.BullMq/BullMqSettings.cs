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
    /// Gets or sets the default options for BullMQ queues.
    /// </summary>
    public required int DatabaseIndex { get; set; } = 4;

    /// <summary>
    /// Gets or sets the default options for BullMQ queues.
    /// </summary>
    public required int RemoveOnCompleteAgeSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets or sets the default options for BullMQ queues.
    /// </summary>
    public required int RemoveOnCompleteCount { get; set; } = 50;

    /// <summary>
    /// Gets or sets the default options for BullMQ queues.
    /// </summary>
    public required int RemoveOnFailAgeSeconds { get; set; } = 12 * 3600;
}
