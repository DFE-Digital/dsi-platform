using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Users.GetServiceUsers;

/// <summary>
/// Request/Query parameters for GetServiceUsers endpoint.
/// </summary>
public record GetServiceUsersQuery(
    [FromQuery(Name = "status")] int? Status = null,
    [FromQuery(Name = "from")] DateTimeOffset? From = null,
    [FromQuery(Name = "to")] DateTimeOffset? To = null,
    [FromQuery(Name = "page")] int Page = 1,
    [FromQuery(Name = "pageSize")] int PageSize = 25
);
