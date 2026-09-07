using System.Text.RegularExpressions;
using Azure.Core;
using Dfe.SignIn.Core.Contracts.Graph;
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
    public async Task ChangePassword(string currentPassword, string newPassword, GraphAccessToken graphAccessToken, CancellationToken cancellationToken = default)
    {
        var accessToken = new AccessToken(
            graphAccessToken.Token,
            graphAccessToken.ExpiresOn
        );

        var graphClient = graphClientFactory.GetClient(accessToken);

        try {
            await graphClient.Me.ChangePassword.PostAsync(new() {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
            }, cancellationToken: cancellationToken);
        }
        catch (ODataError error) {
            var match = GraphErrorMessagePattern().Match(error.Message);
            if (match.Success) {
                string paramName = match.Groups[3].Value;
                string message = match.Groups[1].Value;
                if (paramName == "oldPassword") {
                    throw new FluentValidation.ValidationException([
                        new FluentValidation.Results.ValidationFailure("CurrentPasswordInput", "Please enter your current password")
                    ]);
                }
                else if (paramName == "newPassword") {
                    throw new FluentValidation.ValidationException([
                        new FluentValidation.Results.ValidationFailure("NewPasswordInput", message)
                    ]);
                }
            }
            throw;
        }
    }

    [GeneratedRegex("(.+)( paramName: ([A-Za-z_0-9]+))")]
    private static partial Regex GraphErrorMessagePattern();
}
