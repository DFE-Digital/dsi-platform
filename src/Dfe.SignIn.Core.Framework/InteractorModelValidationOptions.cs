using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.Framework;

/// <summary>
/// Extensions for adding validation for interactor request and response models.
/// </summary>
public sealed class InteractorModelValidationOptions : IOptions<InteractorModelValidationOptions>
{
    /// <summary>
    /// Gets or sets whether request models should be validated.
    /// </summary>
    public bool ValidateRequestModels { get; set; } = true;

    /// <summary>
    /// Gets or sets whether response models should be validated.
    /// </summary>
    public bool ValidateResponseModels { get; set; } = true;

    /// <inheritdoc/>
    InteractorModelValidationOptions IOptions<InteractorModelValidationOptions>.Value => this;
}
