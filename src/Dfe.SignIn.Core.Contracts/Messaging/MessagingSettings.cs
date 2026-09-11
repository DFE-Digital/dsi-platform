namespace Dfe.SignIn.Core.Contracts.Messaging;

/// <summary>
/// Represents the configuration options for messaging in the application.
/// </summary>
public sealed class MessagingSettings
{
    /// <summary>
    /// Gets the name of the configuration section that contains the messaging options.
    /// </summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// Gets or sets a value indicating whether messaging is enabled in the application.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
