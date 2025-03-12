namespace Dfe.SignIn.Core.PublicModels.PublicApiSigning;

/// <summary>
/// A model representing the public keys of the DfE Sign-in public API.
/// </summary>
public sealed record WellKnownPublicKeyListing
{
    /// <summary>
    /// Gets the enumerable collection of public keys.
    /// </summary>
    public required IEnumerable<WellKnownPublicKey> Keys { get; init; }
}

/// <summary>
/// A model representing a public key of the DfE Sign-in public API.
/// </summary>
public sealed record WellKnownPublicKey
{
    /// <summary>
    /// Gets the unique ID representing the public key.
    /// </summary>
    public required string Kid { get; init; }

    /// <summary>
    /// Gets the type of key.
    /// </summary>
    public required string Kty { get; init; }

    /// <summary>
    /// Gets the public key modulus.
    /// </summary>
    public required string N { get; init; }

    /// <summary>
    /// Gets the algorithm.
    /// </summary>
    public required string Alg { get; init; }

    /// <summary>
    /// Gets the public key exponent.
    /// </summary>
    public required string E { get; init; }

    /// <summary>
    /// Gets the expiry date.
    /// </summary>
    public required string Ed { get; init; }

    /// <summary>
    /// Gets a value that indicates how the key is used.
    /// </summary>
    public required string Use { get; init; }
}
