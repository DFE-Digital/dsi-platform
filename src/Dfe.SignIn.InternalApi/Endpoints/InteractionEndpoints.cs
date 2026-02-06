using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.InternalApi.Endpoints;

/// <summary>
/// Internal API interaction endpoints.
/// </summary>
public static partial class InteractionEndpoints
{
    /// <summary>
    /// Registers application interaction endpoints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void UseApplicationEndpoints(this WebApplication app)
    {
        app.Map<GetApplicationApiConfigurationRequest, GetApplicationApiConfigurationResponse>();
    }

    /// <summary>
    /// Registers public API interaction endpoints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void UsePublicApiEndpoints(this WebApplication app)
    {
        app.Map<EncryptPublicApiSecretRequest, EncryptPublicApiSecretResponse>();
        app.Map<DecryptApiSecretRequest, DecryptApiSecretResponse>();
    }

    /// <summary>
    /// Registers user interaction endpoints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void UseUserEndpoints(this WebApplication app)
    {
        app.Map<AutoLinkEntraUserToDsiRequest, AutoLinkEntraUserToDsiResponse>();
        app.Map<CheckIsBlockedEmailAddressRequest, CheckIsBlockedEmailAddressResponse>();
        app.Map<GetUserProfileRequest, GetUserProfileResponse>();
    }
}
