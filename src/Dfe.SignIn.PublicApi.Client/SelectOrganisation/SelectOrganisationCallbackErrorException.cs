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
    /// <param name="errorCode">A value indicating the kind of error that has occurred.</param>
    public SelectOrganisationCallbackErrorException(SelectOrganisationErrorCode errorCode)
    {
        this.ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets a value indicating the kind of error that has occurred.
    /// </summary>
    [Persist]
    public SelectOrganisationErrorCode ErrorCode { get; }
}
