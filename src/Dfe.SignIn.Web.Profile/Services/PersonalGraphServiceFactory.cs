using Azure.Core;
using Dfe.SignIn.Base.Framework;
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
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="personalAccessToken"/> is null.</para>
    /// </exception>
    GraphServiceClient GetClient(AccessToken personalAccessToken);
}

/// <summary>
/// A concrete implementation of the <see cref="IPersonalGraphServiceFactory"/> service.
/// </summary>
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
        ExceptionHelpers.ThrowIfArgumentNull(personalAccessToken, nameof(personalAccessToken));

        return new(new AccessTokenCredential(personalAccessToken));
    }
}
