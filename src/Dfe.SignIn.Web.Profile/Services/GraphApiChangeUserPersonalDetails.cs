using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
/// Graph wrapper to change the user name via entra
/// </summary>
/// <param name="graphClientFactory"></param>
[ExcludeFromCodeCoverage]
public sealed class GraphApiChangeUserPersonalDetails(
    IPersonalGraphServiceFactory graphClientFactory,
    ILogger<GraphApiChangeUserPersonalDetails> logger) : IGraphApiChangeUserPersonalDetails
{
    public async Task ChangeName(Guid userId, string firstName, string lastName, GraphAccessToken? graphAccessToken, CancellationToken cancellationToken = default)
    {
        ExceptionHelpers.ThrowIfArgumentNull(firstName, nameof(firstName));
        ExceptionHelpers.ThrowIfArgumentNull(lastName, nameof(lastName));
        ExceptionHelpers.ThrowIfArgumentNull(graphAccessToken!, nameof(graphAccessToken));

        if (graphAccessToken is null) {
            throw new InvalidOperationException("Missing user access token.");
        }

        if (string.IsNullOrEmpty(firstName)) {
            throw new InvalidOperationException("Missing firstName");
        }

        if (string.IsNullOrEmpty(lastName)) {
            throw new InvalidOperationException("Missing lastName");
        }

        var accessToken = new AccessToken(
           graphAccessToken.Token,
           graphAccessToken.ExpiresOn
       );

        var graphClient = graphClientFactory.GetClient(accessToken);

        try {
            await graphClient.Me.PatchAsync(new User {
                GivenName = firstName,
                Surname = lastName
            });
        }
        catch (ODataError ex) {
            logger.LogError(ex, "Failed to patch user userId: {UserId}", userId);
            throw;
        }
    }
}
