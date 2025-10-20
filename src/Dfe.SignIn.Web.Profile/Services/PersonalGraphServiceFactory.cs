using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Microsoft.Graph;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
/// Represents a factory that provides personalised <see cref="GraphServiceClient"/> instances.
/// </summary>
public interface IPersonalGraphServiceFactory
{
    /// <summary>
    /// Gets a <see cref="GraphServiceClient"/> instance for the given personal access token.
    /// </summary>
    /// <param name="personalAccessToken">The personal access token.</param>
    /// <returns>
    ///   <para>The <see cref="GraphServiceClient"/> instance.</para>
    /// </returns>
    GraphServiceClient GetClient(AccessToken personalAccessToken);
}

/// <summary>
/// A concrete implementation of the <see cref="IPersonalGraphServiceFactory"/> service.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class PersonalGraphServiceFactory : IPersonalGraphServiceFactory
{
    private sealed class AccessTokenCredential(AccessToken personalAccessToken) : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => personalAccessToken;

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => ValueTask.FromResult(personalAccessToken);
    }

    /// <inheritdoc/>
    public GraphServiceClient GetClient(AccessToken personalAccessToken)
    {
        return new(new AccessTokenCredential(personalAccessToken));
    }
}
