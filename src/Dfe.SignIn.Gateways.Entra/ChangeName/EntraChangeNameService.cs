using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Results;
using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace Dfe.SignIn.Gateways.Entra.ChangeName;

/// <summary>
/// Service for changing a user's first and last name in Microsoft Entra ID.
/// </summary>
public interface IEntraChangeNameService
{
    /// <summary>
    /// Changes the first and last name of a user in Microsoft Entra ID.
    /// </summary>
    /// <param name="externalUserId">The external user ID.</param>
    /// <param name="newFirstName">The new first name.</param>
    /// <param name="newLastName">The new last name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    Task<Result> ChangeNameAsync(Guid externalUserId, string newFirstName, string newLastName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of <see cref="IEntraChangeNameService"/> executing Microsoft Graph calls.
/// </summary>
/// <param name="graphClientProvider">The provider for obtaining the Microsoft Graph client.</param>
/// <param name="logger">The logger for logging information and errors.</param>
public sealed class EntraChangeNameService(
    IApplicationGraphClientProvider graphClientProvider,
    ILogger<EntraChangeNameService> logger) : IEntraChangeNameService
{
    /// <inheritdoc/>
    public async Task<Result> ChangeNameAsync(
        Guid externalUserId,
        string newFirstName,
        string newLastName,
        CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentEmpty(externalUserId, nameof(externalUserId));
        ArgumentException.ThrowIfNullOrWhiteSpace(newFirstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(newLastName);
        var client = graphClientProvider.GetClient();
        var userIdString = externalUserId.ToString();
        try {
            var userPatch = new User {
                GivenName = newFirstName,
                Surname = newLastName
            };

            await client.Users[userIdString].PatchAsync(userPatch, cancellationToken: cancellationToken);
            logger.LogInformation("Successfully updated name for Entra user {UserId}", externalUserId);
            return Result.Success();
        }
        catch (Exception ex) {
            if (ex is ODataError oDataEx) {
                var detail = oDataEx.Error?.Message ?? oDataEx.Message;
                logger.LogError(oDataEx,
                    "OData error updating name for Entra user {UserId}: {Code} - {Message}",
                    externalUserId,
                    oDataEx.Error?.Code,
                    detail);
                return Result.Failure(EntraEmailErrors.UserUpdateFailed(detail));
            }
            logger.LogError(ex, "Unexpected error updating name for Entra user {UserId}", externalUserId);
            return Result.Failure(EntraEmailErrors.UserUpdateFailed(ex.Message));
        }
    }
}
