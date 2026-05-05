using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Users.GetServiceUsers;

/// <summary>
/// Query parameters for GetServiceUsers endpoint.
/// </summary>
public record GetServiceUsersQuery(
    [property: FromQuery] int? Status = null,
    [property: FromQuery] DateTimeOffset? From = null,
    [property: FromQuery] DateTimeOffset? To = null,
    [property: FromQuery] int Page = 1,
    [property: FromQuery] int PageSize = 25
);
