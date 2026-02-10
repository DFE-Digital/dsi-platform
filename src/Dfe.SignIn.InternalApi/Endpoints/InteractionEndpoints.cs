using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.Contracts.SupportTickets;
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
    /// Registers organisation interaction endpoints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void UseOrganisationEndpoints(this WebApplication app)
    {
        app.Map<GetOrganisationByIdRequest, GetOrganisationByIdResponse>();
    }

    /// <summary>
    /// Registers public API interaction endpoints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void UsePublicApiEndpoints(this WebApplication app)
    {
        app.Map<EncryptApiSecretRequest, EncryptApiSecretResponse>();
        app.Map<DecryptApiSecretRequest, DecryptApiSecretResponse>();
    }

    /// <summary>
    /// Registers support ticket interaction endpoints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void UseSupportTicketEndpoints(this WebApplication app)
    {
        app.Map<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>();
    }

    /// <summary>
    /// Registers user interaction endpoints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void UseUserEndpoints(this WebApplication app)
    {
        app.Map<AutoLinkEntraUserToDsiRequest, AutoLinkEntraUserToDsiResponse>();
        app.Map<ChangeJobTitleRequest, ChangeJobTitleResponse>();
        app.Map<CheckIsBlockedEmailAddressRequest, CheckIsBlockedEmailAddressResponse>();
        app.Map<GetUserProfileRequest, GetUserProfileResponse>();
    }
}
