using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Configuration;

[TestClass]
public sealed class AuthorizationPolicyExtensionsTests
{
    #region AddDsiPolicies(AuthorizationOptions)

    [TestMethod]
    public void AddDsiPolicies_Throws_WhenOptionsArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => AuthorizationPolicyExtensions.AddDsiPolicies(
                options: null!
            ));
    }

    [TestMethod]
    public void AddDsiPolicies_AddsUserProfilePolicies()
    {
        var options = new AuthorizationOptions();

        AuthorizationPolicyExtensions.AddDsiPolicies(options);

        Assert.IsNotNull(options.GetPolicy(PolicyNames.CanChangeOwnEmailAddress));
        Assert.IsNotNull(options.GetPolicy(PolicyNames.CanChangeOwnPassword));
    }

    #endregion

    #region AddDsiAuthorizationHandlers(IServiceCollection)

    [TestMethod]
    public void AddDsiAuthorizationHandlers_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => AuthorizationPolicyExtensions.AddDsiAuthorizationHandlers(
                services: null!
            ));
    }

    [TestMethod]
    public void AddDsiAuthorizationHandlers_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();

        var result = AuthorizationPolicyExtensions.AddDsiAuthorizationHandlers(services);

        Assert.AreSame(services, result);
    }

    [TestMethod]
    public void AddDsiAuthorizationHandlers_AddsCustomRequirementHandlers()
    {
        var services = new ServiceCollection();

        AuthorizationPolicyExtensions.AddDsiAuthorizationHandlers(services);

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(IAuthorizationHandler) &&
                descriptor.ImplementationType == typeof(InternalUserRequirementHandler)
            )
        );
    }

    #endregion
}
