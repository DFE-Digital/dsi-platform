using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Base.Framework;

namespace Dfe.SignIn.Core.Contracts.Search;

/// <summary>
/// Represents a request to update the user search index.
/// </summary>
[AssociatedResponse(typeof(UpdateUserInSearchIndexResponse))]
public sealed record UpdateUserInSearchIndexRequest
{
    /// <summary>
    /// The unique ID of the user.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }
}

/// <summary>
/// Represents a response for <see cref="UpdateUserInSearchIndexRequest"/>.
/// </summary>
public sealed record UpdateUserInSearchIndexResponse
{

}
