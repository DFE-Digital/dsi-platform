using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

/// <summary>
/// Endpoints for service users, migrated from Node.js getServiceUsers.
/// </summary>
public static partial class UserEndpoints
{
    /// <summary>
    /// Get the service users for a given service (client).
    /// </summary>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <returns>The service users for the given service.</returns>
    public static async Task<IResult> GetServiceUsers(
        IClientSession clientSession,
        IInteractionDispatcher interaction,
        ILoggerFactory loggerFactory,
        HttpContext httpContext)
    {
        var logger = loggerFactory.CreateLogger(nameof(UserEndpoints));
        var correlationId = Activity.Current?.TraceId.ToString();
        var clientCorrelationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        logger.LogInformation(
            "{ClientId} is attempting to get service users (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId})",
            clientSession.ClientId,
            correlationId,
            clientCorrelationId
        );

        //// 1. Extract query parameters
        var pageStr = ExtractParam(httpContext, "page", "1");
        var pageSizeStr = ExtractParam(httpContext, "pageSize", "25");
        //var status = ExtractParam(httpContext, "status");
        //var from = ExtractParam(httpContext, "from");
        //var to = ExtractParam(httpContext, "to");

        var clientId = clientSession.ClientId;

        // 2. Validate page/pageSize
        if (!int.TryParse(pageStr, out var page)) {
            return Results.BadRequest($"{pageStr} is not a valid value for page. Expected a number");
        }
        if (!int.TryParse(pageSizeStr, out var pageSize)) {
            return Results.BadRequest($"{pageSizeStr} is not a valid value for pageSize. Expected a number");
        }

        //DateTime? fromDate = null;
        //DateTime? toDate = null;
        //string? dateRange = null;
        //string? warning = null;
        //const int duration = 90;

        //// 3. Filtered path validation
        //if (!string.IsNullOrEmpty(status) || !string.IsNullOrEmpty(from) || !string.IsNullOrEmpty(to)) {
        //    if (!string.IsNullOrEmpty(status) && status != "0" && status != "1") {
        //        return Results.BadRequest("Status is not valid. Should be either 0 or 1.");
        //    }

        //    if (!string.IsNullOrEmpty(to)) {
        //        if (!DateTime.TryParse(to, out var parsedTo)) {
        //            return Results.BadRequest("To date is not a valid date.");
        //        }
        //        toDate = parsedTo.ToUniversalTime();
        //    }

        //    if (!string.IsNullOrEmpty(from)) {
        //        if (!DateTime.TryParse(from, out var parsedFrom)) {
        //            return Results.BadRequest("From date is not a valid date.");
        //        }
        //        fromDate = parsedFrom.ToUniversalTime();
        //    }

        //    if (fromDate.HasValue && toDate.HasValue) {
        //        if (IsFutureDate(fromDate.Value) && IsFutureDate(toDate.Value)) {
        //            return Results.BadRequest("Date range should not be in the future");
        //        }

        //        if (fromDate.Value > toDate.Value) {
        //            return Results.BadRequest("From date greater than to date");
        //        }

        //        var timeDifference = toDate.Value - fromDate.Value;
        //        if (Math.Abs(timeDifference.TotalDays) > duration) {
        //            return Results.BadRequest($"Only {duration} days are allowed between dates");
        //        }
        //    }
        //    else if (fromDate.HasValue || toDate.HasValue) {
        //        var selectedDate = fromDate ?? toDate;
        //        if (selectedDate.HasValue && IsFutureDate(selectedDate.Value)) {
        //            return Results.BadRequest("Date range should not be in the future");
        //        }
        //    }

        //    // 4. Date auto-fill (findDateRange)
        //    var isWarning = false;
        //    (fromDate, toDate, isWarning) = FindDateRange(toDate, fromDate, duration);

        //    if (isWarning) {
        //        warning = $"Only {duration} days of data can be fetched";
        //    }

        //    if (fromDate.HasValue && toDate.HasValue) {
        //        dateRange = $"Users between {fromDate.Value:ddd, dd MMM yyyy HH:mm:ss} GMT and {toDate.Value:ddd, dd MMM yyyy HH:mm:ss} GMT";
        //    }
        //}

        // 5. Resolve clientId to Application

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

        if (applicationResponse.Application == null) {
            return Results.NotFound();
        }

        // 6. Call the use case
        var response = await interaction.DispatchAsync(
            new GetServiceUsersRequest {
                ApplicationId = applicationResponse.Application.Id,
                PageNumber = page,
                PageSize = pageSize,
                //Status = status,
                //DateFrom = fromDate,
                //DateTo = toDate
            }
        ).To<GetServiceUsersResponse>();

        return Results.Ok(response);
    }

    private static string? ExtractParam(HttpContext context, string name, string? defaultValue = null)
    {
        var key = context.Request.Query.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
        return key != null ? context.Request.Query[key].ToString() : defaultValue;
    }

    //private static bool IsFutureDate(DateTime date)
    //{
    //    return date > DateTime.UtcNow;
    //}

    //private static (DateTime?, DateTime?, bool) FindDateRange(DateTime? toDate, DateTime? fromDate, int duration)
    //{
    //    var isWarning = false;
    //    if (toDate.HasValue && !fromDate.HasValue) {
    //        fromDate = toDate.Value.AddDays(-duration);
    //        isWarning = true;
    //    }
    //    else if (!toDate.HasValue && fromDate.HasValue) {
    //        toDate = fromDate.Value.AddDays(duration);
    //        isWarning = true;
    //    }
    //    else if (!toDate.HasValue && !fromDate.HasValue) {
    //        toDate = DateTime.UtcNow;
    //        fromDate = toDate.Value.AddDays(-duration);
    //        isWarning = true;
    //    }
    //    return (fromDate, toDate, isWarning);
    //}
}
