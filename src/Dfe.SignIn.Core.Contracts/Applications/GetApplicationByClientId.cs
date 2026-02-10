using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Applications;

/// <summary>
/// Request to get an application by its unique client identifier.
/// </summary>
[AssociatedResponse(typeof(GetApplicationByClientIdResponse))]
[Throws(typeof(ApplicationNotFoundException))]
public sealed record GetApplicationByClientIdRequest
{
    /// <summary>
    /// The unique client identifier of the application.
    /// </summary>
    public required string ClientId { get; init; }
}

/// <summary>
/// Response model for interactor <see cref="GetApplicationByClientIdRequest"/>.
/// </summary>
public sealed record GetApplicationByClientIdResponse
{
    /// <summary>
    /// A model representing information about the application.
    /// </summary>
    public required Application Application { get; init; }
}
