using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Interfaces.ExternalAuth;
using Dfe.SignIn.Core.Interfaces.Notifications;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.DistributedCache.Interactions;
using Dfe.SignIn.InternalApi.Endpoints;
using Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi;
using Dfe.SignIn.InternalApi.Features.Users.AutoLinkEntraToDsi.Services;
using Dfe.SignIn.InternalApi.Features.Users.ChangeEmail;
using Dfe.SignIn.InternalApi.Features.Users.ChangeJobTitle;
using Dfe.SignIn.InternalApi.Features.Users.ChangeName;
using Dfe.SignIn.InternalApi.Features.Users.ChangePassword;
using Dfe.SignIn.InternalApi.Features.Users.EmailBlocked;
using Dfe.SignIn.InternalApi.Features.Users.GetUserProfile;
using Dfe.SignIn.InternalApi.Features.Users.IsApprover;
using Dfe.SignIn.InternalApi.Features.Users.PendingApprovalCounter;
using Dfe.SignIn.InternalApi.Features.Users.UserCode;
using Dfe.SignIn.InternalApi.Services.Search;

namespace Dfe.SignIn.InternalApi.Features.Users;

/// <summary>
/// Provides extension methods for mapping user-related endpoints to an <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public static class UsersFeature
{
    private static readonly EndpointRegistry NewEndpointRegistry = new EndpointRegistry()
        .Add<CancelChangeEmailAddressEndpoint>()
        .Add<CheckIsBlockedEmailAddressEndpoint>()
        .Add<ChangePasswordEndpoint>()
        .Add<AutoLinkEntraToDsiEndpoint>();

    /// <summary>
    /// Maps the user-related endpoints to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoints to.</param>
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        ChangeNameEndpoint.Map(app);
        GetUserProfileEndpoint.Map(app);
        InitiateChangeEmailAddressEndpoint.Map(app);
        ConfirmChangeEmailAddressEndpoint.Map(app);
        GetPendingChangeEmailEndpoint.Map(app);
        IsApproverEndpoint.Map(app);
        PendingApprovalCounterEndpoint.Map(app);
        ChangeJobTitleEndpoint.Map(app);

        // New class-based endpoint mapped via registry
        NewEndpointRegistry.MapRoutes(app);

        return app;
    }

    /// <summary>
    /// Adds the <see cref="IUserLookupService"/> to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <param name="configuration">The configuration root to retrieve configuration settings from.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddUserServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddScoped<IUserLookupService, UserLookupService>();
        services.AddScoped<IUserCodeService, UserCodeService>();
        services.AddScoped<IExternalAuthService, StubExternalAuthService>();
        services.AddScoped<IUserUpdatedPublisher, StubUserUpdatedPublisher>();

        services.AddInteractionLimiter<InitiateChangeEmailAddressRequest>(configuration);

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddScoped<RemoveInviteService>();
        services.AddScoped<GenericEmailCheck>();
        services.AddScoped<IUserCreator, UserCreator>();

        // Register class-based endpoints with DI
        NewEndpointRegistry.RegisterServices(services);

        return services;
    }
}
