using System.Text.RegularExpressions;
using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Interfaces.Graph;
using Microsoft.Graph.Models.ODataErrors;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
/// A service that enables a user to change their password with the Graph API.
/// </summary>
public sealed partial class GraphApiChangeUserPassword(
    IPersonalGraphServiceFactory graphClientFactory
) : IGraphApiChangeUserPassword
{
    /// <inheritdoc/>
    public async Task ChangePassword(InteractionContext<SelfChangePasswordRequest> context)
    {
        ExceptionHelpers.ThrowIfArgumentNull(context, nameof(context));

        var request = context.Request;

        if (request.GraphAccessToken is null) {
            throw new InvalidOperationException("Missing user access token.");
        }
        if (request.ConfirmNewPassword != request.NewPassword) {
            // Belts and braces validation since this aspect of the request should
            // have already been validated by the time this method has been called.
            throw new InvalidOperationException("Confirmed password does not match new password.");
        }

        var accessToken = new AccessToken(
            request.GraphAccessToken.Token,
            request.GraphAccessToken.ExpiresOn
        );

        var graphClient = graphClientFactory.GetClient(accessToken);

        try {
            await graphClient.Me.ChangePassword.PostAsync(new() {
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword,
            });
        }
        catch (ODataError error) {
            var match = GraphErrorMessagePattern().Match(error.Message);
            if (match.Success) {
                string paramName = match.Groups[3].Value;
                string message = match.Groups[1].Value;
                if (paramName == "oldPassword") {
                    context.AddValidationError(
                        "Please enter your current password",
                        nameof(request.CurrentPassword)
                    );
                }
                else if (paramName == "newPassword") {
                    context.AddValidationError(message, nameof(request.NewPassword));
                }
                context.ThrowIfHasValidationErrors();
            }
            throw;
        }
    }

    [GeneratedRegex("(.+)( paramName: ([A-Za-z_0-9]+))")]
    private static partial Regex GraphErrorMessagePattern();
}
