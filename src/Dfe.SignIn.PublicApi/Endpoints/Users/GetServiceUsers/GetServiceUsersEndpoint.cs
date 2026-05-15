using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Configuration;

namespace Dfe.SignIn.PublicApi.Endpoints.Users.GetServiceUsers;

/// <summary>
/// Endpoints for service users, migrated from Node.js getServiceUsers.
/// </summary>
public static class GetServiceUsersEndpoint
{
    /// <summary>
    /// Configures the HTTP endpoint for retrieving service users in the specified web application.
    /// </summary>
    /// <remarks>This method registers a GET endpoint at '/users' that returns a list of service users. The
    /// endpoint is documented with OpenAPI metadata and supports both successful and bad request responses. Use this
    /// method during application startup to enable user-related API routes.</remarks>
    /// <param name="app">The endpoint route builder to which the user retrieval endpoint is added.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/users", GetServiceUsers)
            .WithName("GetServiceUsersRequest")
            .WithTags("Users")
            .Produces<GetServiceUsersResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidationFilter<GetServiceUsersQuery>()
            .WithOpenApi();
    }

    /// <summary>
    /// Get the service users for a given service (client).
    /// </summary>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <param name="query">The query parameters for filtering and pagination.</param>
    /// <returns>The service users for the given service.</returns>
    public static async Task<IResult> GetServiceUsers(
        IClientSession clientSession,
        IInteractionDispatcher interaction,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        [AsParameters] GetServiceUsersQuery query)
    {
        var logger = loggerFactory.CreateLogger(nameof(UserEndpoints));
        var correlationId = Activity.Current?.TraceId.ToString();
        var clientCorrelationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        if (logger.IsEnabled(LogLevel.Information)) {
            logger.LogInformation(
                "{ClientId} is attempting to get service users (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId})",
                clientSession.ClientId,
                correlationId,
                clientCorrelationId
            );
        }

        DateTimeOffset? fromDate = query.From;
        DateTimeOffset? toDate = query.To;
        bool isWarning = false;
        if (query.Status.HasValue || query.From.HasValue || query.To.HasValue) {
            (fromDate, toDate, isWarning) = FindDateRange(fromDate, toDate);
        }

        var clientId = clientSession.ClientId;

        GetApplicationByClientIdResponse applicationResponse;

        try {
            applicationResponse = await interaction.DispatchAsync(
                new GetApplicationByClientIdRequest {
                    ClientId = clientId
                }
            ).To<GetApplicationByClientIdResponse>();
        }
        catch (ApplicationNotFoundException) {
            return Results.NotFound($"Application with clientId '{clientId}' not found.");
        }
        catch (Exception ex) {
            logger.LogError(ex, "Unexpected error while retrieving roles for clientId {ClientId}", clientId);
            return Results.Problem("An unexpected error occurred while retrieving application roles.");
        }

        if (applicationResponse?.Application == null) {
            return Results.NotFound();
        }

        var response = await interaction.DispatchAsync(
            new GetServiceUsersRequest {
                ApplicationId = applicationResponse.Application.Id,
                PageNumber = query.Page,
                PageSize = query.PageSize,
                UserStatus = query.Status,
                DateFrom = fromDate,
                DateTo = toDate,
            }
        ).To<GetServiceUsersResponse>();

        string? dateRange = (fromDate.HasValue && toDate.HasValue)
            ? $"Users between {fromDate.Value:R} and {toDate.Value:R}"
            : null;

        string? warning = isWarning
            ? $"Only {GetServiceUsersConstants.MaxDateRangeDays} days of data can be fetched"
            : null;

        return Results.Ok(response with { DateRange = dateRange, Warning = warning });
    }

    private static (DateTimeOffset? fromDate, DateTimeOffset? toDate, bool isWarning) FindDateRange(
        DateTimeOffset? fromDate, DateTimeOffset? toDate)
    {
        if (toDate.HasValue && !fromDate.HasValue) {
            return (toDate.Value.AddDays(-GetServiceUsersConstants.MaxDateRangeDays), toDate, true);
        }

        if (fromDate.HasValue && !toDate.HasValue) {
            return (fromDate, fromDate.Value.AddDays(GetServiceUsersConstants.MaxDateRangeDays), true);
        }

        if (!fromDate.HasValue && !toDate.HasValue) {
            var now = DateTime.UtcNow;
            return (now.AddDays(-GetServiceUsersConstants.MaxDateRangeDays), now, true);
        }

        return (fromDate, toDate, false);
    }
}
