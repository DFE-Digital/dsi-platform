namespace Dfe.SignIn.Core.Contracts.Diagnostics;

/// <summary>
/// Represents a ping/pong request used for testing purposes.
/// </summary>
public sealed record PingRequest
{
    /// <summary>
    /// The requested value.
    /// </summary>
    public required int Value { get; init; }
}

/// <summary>
/// Response model for interactor <see cref="PingRequest"/> used for testing purposes.
/// </summary>
public sealed record PingResponse
{
    /// <summary>
    /// Echo of the requested value.
    /// </summary>
    public required int Value { get; init; }
}
