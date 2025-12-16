using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Applications;

/// <summary>
/// Request to get the Public API client configuration for an application by its
/// unique client identifier.
/// </summary>
/// <remarks>
///   <para>Associated response type:</para>
///   <list type="bullet">
///     <item><see cref="GetApplicationApiConfigurationResponse"/></item>
///   </list>
///   <para>Throws <see cref="ApplicationNotFoundException"/></para>
///   <list type="bullet">
///     <item>When attempting to access an application that does not exist.</item>
///   </list>
/// </remarks>
public sealed record GetApplicationApiConfigurationRequest : IKeyedRequest
{
    /// <summary>
    /// The unique client identifier of the application.
    /// </summary>
    public required string ClientId { get; init; }

    /// <inheritdoc/>
    string IKeyedRequest.Key => this.ClientId;
}

/// <summary>
/// Response model for interactor <see cref="GetApplicationApiConfigurationRequest"/>.
/// </summary>
public sealed record GetApplicationApiConfigurationResponse
{
    /// <summary>
    /// The API client configuration for the application.
    /// </summary>
    public required ApplicationApiConfiguration Configuration { get; init; }
}
