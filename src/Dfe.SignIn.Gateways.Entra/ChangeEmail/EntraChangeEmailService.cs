using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace Dfe.SignIn.Gateways.Entra.ChangeEmail;

/// <summary>
/// Implementation of <see cref="IEntraChangeEmailService"/> executing Microsoft Graph calls.
/// </summary>
public sealed class EntraChangeEmailService(
    IApplicationGraphClientProvider graphClientProvider,
    ILogger<EntraChangeEmailService> logger) : IEntraChangeEmailService
{
    private const string IdentityTypeEmailAddress = "emailAddress";
    private const string IdentityTypeFederated = "federated";
    private const string IdentiesCollectionPropertyName = "identities";

    /// <inheritdoc/>
    public async Task<Base.Framework.OperationResults.OperationResult> ChangeEmailAsync(
        Guid externalUserId,
        string newEmailAddress,
        CancellationToken cancellationToken = default)
    {
        if (externalUserId == Guid.Empty) {
            logger.LogWarning("ChangeEmailAsync rejected: externalUserId is empty.");
            return Base.Framework.OperationResults.OperationResult.Failure(EntraEmailErrors.InvalidUserId);
        }

        if (string.IsNullOrWhiteSpace(newEmailAddress)) {
            logger.LogWarning("ChangeEmailAsync rejected: newEmailAddress is empty or whitespace.");
            return Base.Framework.OperationResults.OperationResult.Failure(EntraEmailErrors.InvalidEmailAddress);
        }

        var client = graphClientProvider.GetClient();
        var userIdString = externalUserId.ToString();
        var normalizedEmail = newEmailAddress.Trim().ToLowerInvariant();

        var primaryEmailResult = await this.UpdatePrimaryEmailAsync(client, userIdString, externalUserId, normalizedEmail, cancellationToken);

        if (!primaryEmailResult.IsSuccess) {
            return primaryEmailResult;
        }

        return await this.UpdateMfaEmailMethodAsync(client, userIdString, externalUserId, normalizedEmail, cancellationToken);
    }

    private async Task<Base.Framework.OperationResults.OperationResult> UpdatePrimaryEmailAsync(
        GraphServiceClient client,
        string userIdString,
        Guid externalUserId,
        string newEmailAddress,
        CancellationToken cancellationToken)
    {
        try {
            var userPatch = new User { Mail = newEmailAddress };

            var entraExternalIdUser = await client.Users[userIdString].GetAsync(config => {
                config.QueryParameters.Select = [IdentiesCollectionPropertyName];
            }, cancellationToken: cancellationToken);

            var identities = entraExternalIdUser?.Identities ?? [];

            // Check if the user has a federated identity type
            bool isFederated = identities.Any(i =>
                string.Equals(i.SignInType, IdentityTypeFederated, StringComparison.OrdinalIgnoreCase));

            if (isFederated) {
                // For federated or external accounts (like corporate logins or social providers), Microsoft Entra ID 
                // does not own or manage the underlying credentials—an external identity provider does. 
                // Entra treats the identities collection for federated accounts as restricted. 
                // Attempting to patch or modify the 'identities' collection via the Graph API violates Entra's 
                // security model, causing it to reject the request and throw the ODataError. 
                // Therefore, we bypass updating userPatch.Identities entirely for these users, leaving their 
                // external trust relationship untouched while still successfully updating their profile contact email above.
                logger.LogInformation("Entra user {UserId} has a federated identity; skipping identity collection update and updating profile 'Mail' only", externalUserId);
            }
            else {
                // For pure local cloud accounts (where Entra fully manages credentials), 
                // it is permitted to update the 'emailAddress' sign-in identity.
                var emailIdentity = identities.FirstOrDefault(i =>
                    string.Equals(i.SignInType, IdentityTypeEmailAddress, StringComparison.OrdinalIgnoreCase));

                if (emailIdentity is null) {
                    logger.LogInformation("Entra user {UserId} has no 'emailAddress' sign-in identity; skipping identity update", externalUserId);
                }
                else if (!string.Equals(emailIdentity.IssuerAssignedId, newEmailAddress, StringComparison.OrdinalIgnoreCase)) {
                    logger.LogInformation("Setting email address the user signs in with for local account");

                    // Reconstruct the identities array, updating only the emailAddress issuerAssignedId to the new address.
                    userPatch.Identities = [.. identities.Select(i => new ObjectIdentity {
                        SignInType = i.SignInType,
                        Issuer = i.Issuer,
                        IssuerAssignedId = string.Equals(i.SignInType, IdentityTypeEmailAddress, StringComparison.OrdinalIgnoreCase)
                            ? newEmailAddress
                            : i.IssuerAssignedId
                    })];
                }
            }

            await client.Users[userIdString].PatchAsync(userPatch, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully updated primary email for Entra user {UserId}", externalUserId);
            return Base.Framework.OperationResults.OperationResult.Success();
        }
        catch (Exception ex) {
            return this.HandleGraphException(ex, externalUserId, "updating primary email", EntraEmailErrors.UserUpdateFailed);
        }
    }

    private async Task<Base.Framework.OperationResults.OperationResult> UpdateMfaEmailMethodAsync(
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

            return Base.Framework.OperationResults.OperationResult.Success();
        }
        catch (Exception ex) {
            return this.HandleGraphException(ex, externalUserId, "updating MFA email authentication method", EntraEmailErrors.MfaAuthenticationMethodFailed);
        }
    }

    private Base.Framework.OperationResults.OperationResult HandleGraphException(
        Exception ex,
        Guid externalUserId,
        string actionContext,
        Func<string, Base.Framework.OperationResults.OperationError> errorFactory)
    {
        if (ex is ODataError oDataEx) {
            var detail = oDataEx.Error?.Message ?? oDataEx.Message;
            logger.LogError(oDataEx,
                "OData error {Context} for Entra user {UserId}: {Code} - {Message}",
                actionContext,
                externalUserId,
                oDataEx.Error?.Code,
                detail);

            return Base.Framework.OperationResults.OperationResult.Failure(errorFactory(detail));
        }

        logger.LogError(ex, "Unexpected error {Context} for Entra user {UserId}", actionContext, externalUserId);
        return Base.Framework.OperationResults.OperationResult.Failure(errorFactory(ex.Message));
    }
}
