namespace Dfe.SignIn.Gateways.GovNotify;

/// <summary>
/// Represents an exception that occurs when there is an error in the notification gateway.
/// </summary>
public class NotificationGatewayException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationGatewayException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NotificationGatewayException(string message, Exception innerException)
        : base(message, innerException) { }
}
