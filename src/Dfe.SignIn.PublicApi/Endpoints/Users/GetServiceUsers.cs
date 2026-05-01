using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

/// <summary>
/// Endpoints for service users, migrated from Node.js getServiceUsers.
/// </summary>
public static partial class UserEndpoints
{
    private const int MaxDateRangeDays = 90;

    /// <summary>
    /// Get the service users for a given service (client).
    /// </summary>
    /// <param name="status">The status filter for the users.</param>
    /// <param name="from">The start date for the date range filter.</param>
    /// <param name="to">The end date for the date range filter.</param>
    /// <param name="page">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page for pagination.</param>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <returns>The service users for the given service.</returns>
    public static async Task<IResult> GetServiceUsers(
        IClientSession clientSession,
        IInteractionDispatcher interaction,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        [FromQuery] int? status = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
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

        // Validate page/pageSize
        if (page < 1 || pageSize < 1) {
            return Results.BadRequest("page and pageSize must be greater than 0.");
        }

        DateTimeOffset? fromDate = null;
        DateTimeOffset? toDate = null;
        bool isWarning = false;

        if (status.HasValue || from.HasValue || to.HasValue) {

            // Validate status
            if (status.HasValue && status != 0 && status != 1) {
                return Results.BadRequest("Status is not valid. Should be either 0 or 1.");
            }

            // Validate dates
            fromDate = from;
            toDate = to;

            if (fromDate.HasValue && toDate.HasValue) {
                if (fromDate.Value > DateTime.UtcNow && toDate.Value > DateTime.UtcNow) {
                    return Results.BadRequest("Date range should not be in the future.");
                }
                if (fromDate.Value > toDate.Value) {
                    return Results.BadRequest("From date greater than to date.");
                }
                var daysDifference = (toDate.Value - fromDate.Value).TotalDays;
                if (daysDifference > MaxDateRangeDays) {
                    return Results.BadRequest($"Only {MaxDateRangeDays} days are allowed between dates.");
                }
            }
            else if (fromDate.HasValue || toDate.HasValue) {
                var selectedDate = fromDate ?? toDate;
                if (selectedDate!.Value > DateTime.UtcNow) {
                    return Results.BadRequest("Date range should not be in the future.");
                }
            }

            // Fill in missing date bounds (mirrors Node findDateRange), set warning if inferred
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
                PageNumber = page,
                PageSize = pageSize,
                UserStatus = status,
                DateFrom = fromDate,
                DateTo = toDate,
            }
        ).To<GetServiceUsersResponse>();

        string? dateRange = (fromDate.HasValue && toDate.HasValue)
            ? $"Users between {fromDate.Value:R} and {toDate.Value:R}"
            : null;

        string? warning = isWarning
            ? $"Only {MaxDateRangeDays} days of data can be fetched"
            : null;

        return Results.Ok(response with { DateRange = dateRange, Warning = warning });
    }

    private static (DateTimeOffset? fromDate, DateTimeOffset? toDate, bool isWarning) FindDateRange(
        DateTimeOffset? fromDate, DateTimeOffset? toDate)
    {
        if (toDate.HasValue && !fromDate.HasValue) {
            return (toDate.Value.AddDays(-MaxDateRangeDays), toDate, true);
        }
        if (fromDate.HasValue && !toDate.HasValue) {
            return (fromDate, fromDate.Value.AddDays(MaxDateRangeDays), true);
        }
        if (!fromDate.HasValue && !toDate.HasValue) {
            var now = DateTime.UtcNow;
            return (now.AddDays(-MaxDateRangeDays), now, true);
        }
        return (fromDate, toDate, false);
    }
}
