using Dfe.SignIn.InternalApi.Feature.Applications.GetApplicationByClientId;

namespace Dfe.SignIn.InternalApi.Feature.Applications;

/// <summary>
/// Provides extension methods for mapping application-related endpoints in the internal API.
/// </summary>
public static class ApplicationEndpoints
{
    /// <inheritdoc/>
    public static void UseApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        GetApplicationByClientIdEndpoint.Map(app);
    }
}
