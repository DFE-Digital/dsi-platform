using Dfe.SignIn.Base.Framework.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace Dfe.SignIn.Gateways.Entra.ChangeEmail;

/// <summary>
/// Service for updating user email addresses and MFA methods in Microsoft Entra ID.
/// </summary>
public interface IEntraChangeEmailService
{
    /// <summary>
    /// Synchronizes an email address change to Microsoft Entra ID.
    /// Performs:
    /// 1. Primary user email update (PATCH users/{userId}).
    /// 2. Entra MFA email authentication method update (GET / PATCH / POST users/{userId}/authentication/emailMethods).
    /// </summary>
    /// <param name="externalUserId">The Entra OID of the user.</param>
    /// <param name="newEmailAddress">The new email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result"/> indicating success or the specific failure error.</returns>
    Task<Result> ChangeEmailAsync(Guid externalUserId, string newEmailAddress, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of <see cref="IEntraChangeEmailService"/> executing Microsoft Graph calls.
/// </summary>
public sealed class EntraChangeEmailService(
    IApplicationGraphServiceFactory graphServiceFactory,
    ILogger<EntraChangeEmailService> logger) : IEntraChangeEmailService
{
    /// <summary>
    /// Synchronizes an email address change to Microsoft Entra ID.
    /// </summary>
    /// <param name="externalUserId">The Entra OID of the user.</param>
    /// <param name="newEmailAddress">The new email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Result"/> indicating success or the specific failure error.</returns>
    public async Task<Result> ChangeEmailAsync(
        Guid externalUserId,
        string newEmailAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmailAddress);

        var client = graphServiceFactory.CreateClient();
        var userIdString = externalUserId.ToString();

        // ------------------------------------------------------------------
        // Step 1: Update primary user email on Entra User object
        // ------------------------------------------------------------------
        try {
            var userPatch = new User {
                Mail = newEmailAddress
            };

            await client.Users[userIdString].PatchAsync(userPatch, cancellationToken: cancellationToken);
            logger.LogInformation("Successfully updated primary email for Entra user {UserId}", externalUserId);
        }
        catch (ODataError oDataEx) {
            var detail = oDataEx.Error?.Message ?? oDataEx.Message;
            logger.LogError(oDataEx, "OData error updating primary email for Entra user {UserId}: {Code} - {Message}",
                externalUserId, oDataEx.Error?.Code, detail);
            return Result.Failure(EntraEmailErrors.UserUpdateFailed(detail));
        }
        catch (Exception ex) {
            logger.LogError(ex, "Unexpected error updating primary email for Entra user {UserId}", externalUserId);
            return Result.Failure(EntraEmailErrors.UserUpdateFailed(ex.Message));
        }

        // ------------------------------------------------------------------
        // Step 2: Update Entra MFA Email Authentication Method
        // ------------------------------------------------------------------
        try {
            var emailMethods = await client.Users[userIdString]
                .Authentication
                .EmailMethods
                .GetAsync(cancellationToken: cancellationToken);

            var existingMethod = emailMethods?.Value?.FirstOrDefault();

            if (existingMethod is not null && !string.IsNullOrEmpty(existingMethod.Id)) {
                var methodPatch = new EmailAuthenticationMethod {
                    EmailAddress = newEmailAddress
                };

                await client.Users[userIdString]
                    .Authentication
                    .EmailMethods[existingMethod.Id]
                    .PatchAsync(methodPatch, cancellationToken: cancellationToken);

                logger.LogInformation("Successfully updated existing MFA email method for Entra user {UserId}", externalUserId);
            } else {
                var newMethod = new EmailAuthenticationMethod {
                    EmailAddress = newEmailAddress
                };

                await client.Users[userIdString]
                    .Authentication
                    .EmailMethods
                    .PostAsync(newMethod, cancellationToken: cancellationToken);

                logger.LogInformation("Successfully created new MFA email method for Entra user {UserId}", externalUserId);
            }
        }
        catch (ODataError oDataEx) {
            var detail = oDataEx.Error?.Message ?? oDataEx.Message;
            logger.LogError(oDataEx, "Failed to update MFA email authentication method in Entra for user {UserId}: {Code} - {Message}",
                externalUserId, oDataEx.Error?.Code, detail);
            return Result.Failure(EntraEmailErrors.MfaAuthenticationMethodFailed(detail));
        }
        catch (Exception ex) {
            logger.LogError(ex, "Unexpected error updating MFA email authentication method in Entra for user {UserId}", externalUserId);
            return Result.Failure(EntraEmailErrors.MfaAuthenticationMethodFailed(ex.Message));
        }

        return Result.Success();
    }
}
