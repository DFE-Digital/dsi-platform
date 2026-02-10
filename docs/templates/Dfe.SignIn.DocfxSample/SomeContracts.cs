using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.DocfxSample;

/// <summary>
/// An example type of request contract.
/// </summary>
[AssociatedResponse(typeof(SomeResponse))]
[Throws(typeof(SomeException))]
public sealed record SomeRequest { }

/// <summary>
/// An example type of response contract.
/// </summary>
public sealed record SomeResponse { }

/// <summary>
/// An example exception that can be thrown.
/// </summary>
public sealed class SomeException : Exception { }
