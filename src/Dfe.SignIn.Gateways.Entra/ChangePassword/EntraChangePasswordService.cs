using System.Text.RegularExpressions;
using Azure.Core;
using Dfe.SignIn.Base.Framework.OperationResults;
using Dfe.SignIn.Core.Contracts.Graph;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;

namespace Dfe.SignIn.Gateways.Entra.ChangePassword;

/// <summary>
/// A service that enables an Entra user to change their password via the Graph API using delegated permissions.
/// </summary>
public interface IEntraChangePasswordService
{
    /// <summary>
    /// Changes the user's password using the provided delegated access token.
    /// </summary>
    /// <param name="currentPassword">The user's current password.</param>
    /// <param name="newPassword">The user's new password.</param>
    /// <param name="graphAccessToken">The delegated Graph access token for the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A OperationResult indicating success or failure.</returns>
    Task<OperationResult> ChangePasswordAsync(string currentPassword, string newPassword, GraphAccessToken graphAccessToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// A service that enables an Entra user to change their password via the Graph API using delegated permissions.
/// </summary>
/// <param name="logger">The logger instance.</param>
public sealed partial class EntraChangePasswordService(ILogger<EntraChangePasswordService> logger) : IEntraChangePasswordService
{
    private sealed class AccessTokenCredential(GraphAccessToken token) : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => new(token.Token, token.ExpiresOn);

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => ValueTask.FromResult(new AccessToken(token.Token, token.ExpiresOn));
    }

    /// <inheritdoc/>
    public async Task<OperationResult> ChangePasswordAsync(string currentPassword, string newPassword, GraphAccessToken graphAccessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentPassword)) {
            logger.LogWarning("ChangePasswordAsync rejected: currentPassword is empty.");
            return OperationResult.Failure(EntraPasswordErrors.InvalidCurrentPassword);
        }

        if (string.IsNullOrWhiteSpace(newPassword)) {
            logger.LogWarning("ChangePasswordAsync rejected: newPassword is empty.");
            return OperationResult.Failure(EntraPasswordErrors.PasswordPolicyViolation("New password cannot be empty."));
        }

        if (graphAccessToken is null || string.IsNullOrWhiteSpace(graphAccessToken.Token)) {
            logger.LogWarning("ChangePasswordAsync rejected: graphAccessToken is null or empty.");
            return OperationResult.Failure(EntraPasswordErrors.Unexpected("Missing access token."));
        }

        try {
            var credential = new AccessTokenCredential(graphAccessToken);
            var graphClient = new GraphServiceClient(credential);

            await graphClient.Me.ChangePassword.PostAsync(new() {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            }, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully changed password for Entra user.");
            return OperationResult.Success();
        }
        catch (ODataError oDataEx) {
            var message = oDataEx.Error?.Message ?? oDataEx.Message;
            logger.LogError(oDataEx, "OData error changing password for Entra user: {Code} - {Message}", oDataEx.Error?.Code, message);

            var match = GraphErrorMessagePattern().Match(message);
            if (match.Success) {
                string paramName = match.Groups[3].Value;
                string detailMessage = match.Groups[1].Value;

                if (string.Equals(paramName, "oldPassword", StringComparison.OrdinalIgnoreCase)) {
                    return OperationResult.Failure(EntraPasswordErrors.InvalidCurrentPassword);
                }
                if (string.Equals(paramName, "newPassword", StringComparison.OrdinalIgnoreCase)) {
                    return OperationResult.Failure(EntraPasswordErrors.PasswordPolicyViolation(detailMessage));
                }
            }

            return OperationResult.Failure(EntraPasswordErrors.Unexpected(message));
        }
        catch (Exception ex) {
            logger.LogError(ex, "Unexpected error changing password for Entra user.");
            return OperationResult.Failure(EntraPasswordErrors.Unexpected(ex.Message));
        }
    }

    [GeneratedRegex("(.+)( paramName: ([A-Za-z_0-9]+))")]
    private static partial Regex GraphErrorMessagePattern();
}
