using System.ComponentModel.DataAnnotations;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.InternalApi.Configuration;
using Dfe.SignIn.InternalApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.InternalApi.Features.Users.EmailBlocked;

/// <summary>
/// Endpoint for determing if an email address is valid
/// </summary>
/// <param name="blockedEmailAddressOptions">Configuration options containing the values for blacklisting</param>
/// <param name="logger"></param>
public sealed class CheckIsBlockedEmailAddressEndpoint(
    IOptions<BlockedEmailAddressOptions> blockedEmailAddressOptions,
    ILogger<CheckIsBlockedEmailAddressEndpoint> logger) : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.CheckEmailBlocked, (
            [FromBody] CheckIsBlockedEmailAddressRequest emailAddressToValidate,
             [FromServices] CheckIsBlockedEmailAddressEndpoint endpoint,
            CancellationToken cancellationToken) =>
            endpoint.Handle(emailAddressToValidate))
            .WithName("Validates provided email against blacklist")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithOpenApi();
    }

    /// <summary>
    /// Handler to determine if the email address provided is blacklisted.
    /// </summary>
    /// <param name="request">value to check</param>
    /// <returns></returns>
    public IResult Handle(
        CheckIsBlockedEmailAddressRequest request)
    {
        logger.LogInformation("Determining if provided email is blacklisted.");

        if (string.IsNullOrEmpty(request.EmailAddress)) {
            return Results.BadRequest("Email address is required.");
        }

        var isValid = Core.Contracts.StringPatterns.EmailAddressRegex()
    .IsMatch(request.EmailAddress);

        if (!isValid) {
            return Results.BadRequest("Email address is invalid.");
        }

        var parts = request.EmailAddress.Split('@');

        var localPart = parts[0];
        var domain = parts[1];

        var blockedDomains = blockedEmailAddressOptions.Value.BlockedDomains;
        var blockedNames = blockedEmailAddressOptions.Value.BlockedNames;

        var isBlockedDomain = blockedDomains.Contains(domain, StringComparer.OrdinalIgnoreCase);
        var isBlockedName = blockedNames.Any(blockedName => {
            if (!localPart.StartsWith(blockedName, StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            if (localPart.Length == blockedName.Length) {
                return true;
            }

            //"Does the email local part equal a blocked name,
            //or start with a blocked name followed by a digit,
            //dot, hyphen, or underscore?"
            char nextChar = localPart[blockedName.Length];
            return char.IsDigit(nextChar)
                || nextChar is '.' or '-' or '_';
        });

        logger.LogInformation("Email address blacklist check completed.");

        return Results.Ok(new CheckIsBlockedEmailAddressResponse {
            IsBlocked = isBlockedDomain || isBlockedName,
        });
    }
}
