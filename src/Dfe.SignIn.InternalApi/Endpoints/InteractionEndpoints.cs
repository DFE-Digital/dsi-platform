using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.InternalApi.Endpoints;

/// <summary>
/// Internal API interaction endpoints.
/// </summary>
public static partial class InteractionEndpoints
{
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
