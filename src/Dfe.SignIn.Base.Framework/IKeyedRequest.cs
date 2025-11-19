namespace Dfe.SignIn.Base.Framework;

/// <summary>
/// Constant values for <see cref="IKeyedRequest"/> implementations.
/// </summary>
public static class KeyedRequestConstants
{
    /// <summary>
    /// A default key suitable for request types that do not include any parameters or
    /// distinguishing characteristics.
    /// </summary>
    public const string DefaultKey = "default";
}

/// <summary>
/// Indicates that a request has a unique identity key.
/// </summary>
public interface IKeyedRequest
{
    /// <summary>
    /// Gets the unique key that identifies this request instance.
    /// This key can be used for operations such as caching, throttling, or deduplication.
    /// </summary>
    string Key { get; }
}
