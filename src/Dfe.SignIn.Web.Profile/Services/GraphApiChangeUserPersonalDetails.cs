using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
/// 
/// </summary>
/// <param name="graphClientFactory"></param>
public sealed class GraphApiChangeUserPersonalDetails(
    IPersonalGraphServiceFactory graphClientFactory,
    ILogger<GraphApiChangeUserPersonalDetails> logger) : IGraphApiChangeUserPersonalDetails
{
    public async Task ChangeName(Guid userId, string forename, string lastName, GraphAccessToken? graphAccessToken)
    {
        ExceptionHelpers.ThrowIfArgumentNull(forename, nameof(forename));
        ExceptionHelpers.ThrowIfArgumentNull(lastName, nameof(lastName));
        ExceptionHelpers.ThrowIfArgumentNull(graphAccessToken!, nameof(graphAccessToken));

        if (graphAccessToken is null) {
            throw new InvalidOperationException("Missing user access token.");
        }

        if (string.IsNullOrEmpty(forename)) {
            throw new InvalidOperationException("Missing forname");
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
                GivenName = forename,
                Surname = lastName
            });
        }
        catch (ODataError ex) {
            logger.LogError(ex, "Failed to patch user userId: {userId}", userId);
            throw;
        }
    }
}
