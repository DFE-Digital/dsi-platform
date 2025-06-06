using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;

namespace Dfe.SignIn.PublicApi.Client.SelectOrganisation;

/// <summary>
/// The exception thrown when a "select organisation" callback error occurs.
/// </summary>
[Serializable]
public sealed class SelectOrganisationCallbackErrorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectOrganisationCallbackErrorException"/> class.
    /// </summary>
    public SelectOrganisationCallbackErrorException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectOrganisationCallbackErrorException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public SelectOrganisationCallbackErrorException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectOrganisationCallbackErrorException"/> class.
    /// </summary>
    /// <inheritdoc/>
    public SelectOrganisationCallbackErrorException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectOrganisationCallbackErrorException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="errorCode">A value indicating the kind of error that has occurred.</param>
    public SelectOrganisationCallbackErrorException(string? message, string errorCode)
        : base(message)
    {
        this.ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets a value indicating the kind of error that has occurred.
    /// </summary>
    /// <seealso cref="SelectOrganisationErrorCode"/>
    [Persist]
    public string ErrorCode { get; } = SelectOrganisationErrorCode.InternalError;
}
