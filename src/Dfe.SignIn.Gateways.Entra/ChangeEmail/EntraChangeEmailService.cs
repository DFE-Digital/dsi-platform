using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Base.Framework.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace Dfe.SignIn.Gateways.Entra.ChangeEmail;

/// <summary>
/// Service for updating user email addresses and MFA methods in Microsoft Entra ID.
/// </summary>
public interface IEntraChangeEmailService
{
    /// <summary>
    /// Changes the primary email address and updates the MFA email authentication method for a user in Microsoft Entra ID.
    /// </summary>
    /// <param name="externalUserId">The external user ID.</param>
    /// <param name="newEmailAddress">The new email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    /// <remarks>
    /// Primary email may succeed while MFA email-method update fails. Callers that treat
    /// <see cref="EntraEmailErrors.MfaAuthenticationMethodFailedCode"/> as a soft failure
    /// should expect Entra primary mail to already reflect <paramref name="newEmailAddress"/>.
    /// </remarks>
    Task<Result> ChangeEmailAsync(Guid externalUserId, string newEmailAddress, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of <see cref="IEntraChangeEmailService"/> executing Microsoft Graph calls.
/// </summary>
public sealed class EntraChangeEmailService(
    IApplicationGraphClientProvider graphClientProvider,
    ILogger<EntraChangeEmailService> logger) : IEntraChangeEmailService
{
    /// <inheritdoc/>
    public async Task<Result> ChangeEmailAsync(
        Guid externalUserId,
        string newEmailAddress,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmailAddress);
        ExceptionHelpers.ThrowIfArgumentEmpty(externalUserId, nameof(externalUserId));

        var client = graphClientProvider.GetClient();
        var userIdString = externalUserId.ToString();

        var primaryEmailResult = await this.UpdatePrimaryEmailAsync(client, userIdString, externalUserId, newEmailAddress, cancellationToken);

        if (!primaryEmailResult.IsSuccess) {
            return primaryEmailResult;
        }

        return await this.UpdateMfaEmailMethodAsync(client, userIdString, externalUserId, newEmailAddress, cancellationToken);
    }

    private async Task<Result> UpdatePrimaryEmailAsync(
        GraphServiceClient client,
        string userIdString,
        Guid externalUserId,
        string newEmailAddress,
        CancellationToken cancellationToken)
    {
        try {
            var userPatch = new User { Mail = newEmailAddress };
            await client.Users[userIdString].PatchAsync(userPatch, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully updated primary email for Entra user {UserId}", externalUserId);
            return Result.Success();
        }
        catch (Exception ex) {
            return this.HandleGraphException(ex, externalUserId, "updating primary email", EntraEmailErrors.UserUpdateFailed);
        }
    }

    private async Task<Result> UpdateMfaEmailMethodAsync(
        GraphServiceClient client,
        string userIdString,
        Guid externalUserId,
        string newEmailAddress,
        CancellationToken cancellationToken)
    {
        try {
            var emailMethods = await client.Users[userIdString]
                .Authentication
                .EmailMethods
                .GetAsync(cancellationToken: cancellationToken);

            var existingMethod = emailMethods?.Value?.FirstOrDefault();

            if (existingMethod is not null && !string.IsNullOrEmpty(existingMethod.Id)) {
                var methodPatch = new EmailAuthenticationMethod { EmailAddress = newEmailAddress };
                await client.Users[userIdString]
                    .Authentication
                    .EmailMethods[existingMethod.Id]
                    .PatchAsync(methodPatch, cancellationToken: cancellationToken);

                logger.LogInformation("Successfully updated existing MFA email method for Entra user {UserId}", externalUserId);
            }
            else {
                var newMethod = new EmailAuthenticationMethod { EmailAddress = newEmailAddress };
                await client.Users[userIdString]
                    .Authentication
                    .EmailMethods
                    .PostAsync(newMethod, cancellationToken: cancellationToken);

                logger.LogInformation("Successfully created new MFA email method for Entra user {UserId}", externalUserId);
            }

            return Result.Success();
        }
        catch (Exception ex) {
            return this.HandleGraphException(ex, externalUserId, "updating MFA email authentication method", EntraEmailErrors.MfaAuthenticationMethodFailed);
        }
    }

    private Result HandleGraphException(
        Exception ex,
        Guid externalUserId,
        string actionContext,
        Func<string, Error> errorFactory)
    {
        if (ex is ODataError oDataEx) {
            var detail = oDataEx.Error?.Message ?? oDataEx.Message;
            logger.LogError(oDataEx,
                "OData error {Context} for Entra user {UserId}: {Code} - {Message}",
                actionContext,
                externalUserId,
                oDataEx.Error?.Code,
                detail);

            return Result.Failure(errorFactory(detail));
        }

        logger.LogError(ex, "Unexpected error {Context} for Entra user {UserId}", actionContext, externalUserId);
        return Result.Failure(errorFactory(ex.Message));
    }
}
