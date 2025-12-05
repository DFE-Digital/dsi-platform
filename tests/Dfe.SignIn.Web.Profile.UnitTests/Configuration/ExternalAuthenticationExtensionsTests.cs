using Dfe.SignIn.Web.Profile.Configuration;
using Dfe.SignIn.Web.Profile.Services;
using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.TokenCacheProviders;

namespace Dfe.SignIn.Web.Profile.UnitTests.Configuration;

[TestClass]
public sealed class ExternalAuthenticationExtensionsTests
{
    #region AddExternalAuthentication(IServiceCollection, IConfigurationRoot)

    private static IConfigurationRoot CreateFakeConfigurationForAddExternalAuthentication()
        => new ConfigurationBuilder()
            .AddInMemoryCollection([
                new($"{ExternalAuthenticationExtensions.TokenRedisCacheConfigurationSectionName}:ConnectionString", "localhost"),
                new($"{ExternalAuthenticationExtensions.TokenRedisCacheConfigurationSectionName}:DatabaseNumber", "1"),
            ])
            .AddInMemoryCollection([
                new($"{UserSessionExtensions.SessionConfigurationSectionName}:DurationInMinutes", "25"),
            ])
            .Build();

    [TestMethod]
    public void AddExternalAuthentication_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ExternalAuthenticationExtensions.AddExternalAuthentication(
                services: null!,
                configuration: new ConfigurationBuilder().Build()
            ));
    }

    [TestMethod]
    public void AddExternalAuthentication_Throws_WhenConfigurationArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => ExternalAuthenticationExtensions.AddExternalAuthentication(
                services: new ServiceCollection(),
                configuration: null!
            ));
    }

    [TestMethod]
    public void AddExternalAuthentication_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddExternalAuthentication();

        var result = ExternalAuthenticationExtensions.AddExternalAuthentication(services, configuration);

        Assert.AreSame(result, services);
    }

    [TestMethod]
    public void AddExternalAuthentication_AddsAuthenticationServices()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddExternalAuthentication();

        ExternalAuthenticationExtensions.AddExternalAuthentication(services, configuration);

        Assert.IsTrue(
            services.Any(descriptor => descriptor.ServiceType == typeof(ISelectAssociatedAccountHelper))
        );
        Assert.IsTrue(
            services.Any(descriptor => descriptor.ServiceType == typeof(IAuthenticationService))
        );
        Assert.IsTrue(
            services.Any(descriptor => descriptor.ServiceType == typeof(IMsalTokenCacheProvider))
        );
    }

    [TestMethod]
    public void AddExternalAuthentication_ConfiguresAuthenticationCookie()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddExternalAuthentication();

        ExternalAuthenticationExtensions.AddExternalAuthentication(services, configuration);

        var provider = services.BuildServiceProvider();

        var optionsAccessor = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var options = optionsAccessor.Get(ExternalAuthConstants.CookiesSchemeName);

        Assert.AreEqual(ExternalAuthConstants.CookieName, options.Cookie.Name);
        Assert.AreEqual(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.AreEqual(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.IsTrue(options.Cookie.HttpOnly);
        Assert.IsTrue(options.Cookie.IsEssential);
        Assert.AreEqual(25, options.ExpireTimeSpan.TotalMinutes);
    }

    #endregion
}
