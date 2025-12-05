using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.WebFramework.Mvc.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Dfe.SignIn.WebFramework.Mvc.Configuration;

/// <summary>
/// Provides extension methods for configuring authorization policies and
/// registering authorization handlers which can be used by the applications
/// that form the DfE Sign-In platform.
/// </summary>
public static class AuthorizationPolicyExtensions
{
    /// <summary>
    /// Adds all authorization policies relating to the DfE Sign-In platform.
    /// </summary>
    /// <param name="options">The authorization options that are being configured.</param>
    /// <exception cref="ArgumentException">
    ///   <para>If <paramref name="options"/> is null.</para>
    /// </exception>
    public static void AddDsiPolicies(this AuthorizationOptions options)
    {
        ExceptionHelpers.ThrowIfArgumentNull(options, nameof(options));

        AddUserProfilePolicies(options);
    }

    private static void AddUserProfilePolicies(AuthorizationOptions options)
    {
        options.AddPolicy(PolicyNames.CanChangeOwnEmailAddress, policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new InternalUserRequirement { IsInternalUser = false })
        );

        options.AddPolicy(PolicyNames.CanChangeOwnPassword, policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new InternalUserRequirement { IsInternalUser = false })
        );
    }

    /// <summary>
    /// Adds default <see cref="IAuthorizationHandler"/> implementations to evaluate
    /// custom <see cref="IAuthorizationRequirement"/>'s that are used by the policies
    /// that are added by <see cref="AddDsiPolicies"/>.
    /// </summary>
    /// <param name="services">The collection of services to add to.</param>
    /// <returns>
    ///   <para>The <paramref name="services"/> instance for chained calls.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static IServiceCollection AddDsiAuthorizationHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, InternalUserRequirementHandler>();

        return services;
    }
}
