namespace Dfe.SignIn.WebFramework;

/// <summary>
/// Defines the possible deployment environments in which the application can run.
/// </summary>
public enum EnvironmentName
{
    /// <summary>
    /// Represents a developer’s local machine environment.
    /// </summary>
    Local = 0,

    /// <summary>
    /// Represents a development environment.
    /// </summary>
    Dev,

    /// <summary>
    /// Represents a testing environment.
    /// </summary>
    Test,

    /// <summary>
    /// Represents the transformation environment.
    /// </summary>
    Tran,

    /// <summary>
    /// Represents a pre-production environment.
    /// </summary>
    PreProd,

    /// <summary>
    /// Represents the live production environment.
    /// </summary>
    Prod
}
