using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Mvc.Configuration;

/// <summary>
/// Options for user sessions.
/// </summary>
public sealed class UserSessionOptions : IOptions<UserSessionOptions>
{
    /// <summary>
    /// Gets the duration of a user session in minutes.
    /// </summary>
    public required double DurationInMinutes { get; set; } = 0;

    /// <summary>
    /// Gets the number of remaining minutes at which the session timeout modal
    /// is presented to the user.
    /// </summary>
    public required double NotifyRemainingMinutes { get; set; } = 5;

    /// <inheritdoc/>
    UserSessionOptions IOptions<UserSessionOptions>.Value => this;
}
