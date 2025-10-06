namespace Dfe.SignIn.Core.Contracts.Applications;

/// <summary>
/// Request to get an application by its unique client identifier.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetApplicationByClientIdResponse"/></item>
///   </list>
///   <para>Throws <see cref="ApplicationNotFoundException"/></para>
///   <list type="bullet">
///     <item>When attempting to access an application that does not exist.</item>
///   </list>
/// </remarks>
public sealed record GetApplicationByClientIdRequest
{
    /// <summary>
    /// Gets the unique client identifier of the application.
    /// </summary>
    public required string ClientId { get; init; }
}

/// <summary>
/// Response model for interactor <see cref="GetApplicationByClientIdRequest"/>.
/// </summary>
public sealed record GetApplicationByClientIdResponse
{
    /// <summary>
    /// Gets a model representing information about the application.
    /// </summary>
    public required Application Application { get; init; }
}
