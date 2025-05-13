namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// The exception that occurs when a "select organisation" callback is being processed
/// which does not belong to the current primary identity.
/// </summary>
/// <remarks>
///   <para>This can occur in the following scenarios:</para>
///   <list type="bullet">
///     <item>The user signs into a different account using a different tab in their
///     web browser before selecting an organisation.</item>
///     <item>The user re-submits a callback form in another tab after having signed
///     into a different account.</item>
///   </list>
/// </remarks>
public sealed class MismatchedCallbackException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MismatchedCallbackException"/> class.
    /// </summary>
    public MismatchedCallbackException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MismatchedCallbackException"/> class.
    /// </summary>
    /// <inheritdoc />
    public MismatchedCallbackException(string? message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MismatchedCallbackException"/> class.
    /// </summary>
    /// <inheritdoc />
    public MismatchedCallbackException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
