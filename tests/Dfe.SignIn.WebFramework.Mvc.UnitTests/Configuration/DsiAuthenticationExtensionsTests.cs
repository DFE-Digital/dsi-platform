using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Configuration;

[TestClass]
public sealed class DsiAuthenticationExtensionsTests
{
    #region AddDsiAuthentication(IServiceCollection, IConfigurationRoot)

    private static IConfigurationRoot CreateFakeConfigurationForAddDsiAuthentication()
        => new ConfigurationBuilder()
            .AddInMemoryCollection([
                new($"{DsiAuthenticationExtensions.OidcConfigurationSectionName}:ClientId", "TestClientName"),
            ])
            .AddInMemoryCollection([
                new($"{UserSessionExtensions.SessionConfigurationSectionName}:DurationInMinutes", "25"),
            ])
            .Build();

    [TestMethod]
    public void AddDsiAuthentication_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => DsiAuthenticationExtensions.AddDsiAuthentication(
                services: null!,
                configuration: new ConfigurationBuilder().Build()
            ));
    }

    [TestMethod]
    public void AddDsiAuthentication_Throws_WhenConfigurationArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => DsiAuthenticationExtensions.AddDsiAuthentication(
                services: new ServiceCollection(),
                configuration: null!
            ));
    }

    [TestMethod]
    public void AddDsiAuthentication_ReturnsServicesForChainedCalls()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddDsiAuthentication();

        var result = DsiAuthenticationExtensions.AddDsiAuthentication(services, configuration);

        Assert.AreSame(result, services);
    }

    [TestMethod]
    public void AddDsiAuthentication_MapsApplicationOidcOptions()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddDsiAuthentication();

        DsiAuthenticationExtensions.AddDsiAuthentication(services, configuration);

        var provider = services.BuildServiceProvider();
        var optionsAccessor = provider.GetRequiredService<IOptions<ApplicationOidcOptions>>();
        var options = optionsAccessor.Value;

        Assert.AreEqual("TestClientName", options.ClientId);
    }

    [TestMethod]
    public void AddDsiAuthentication_AddsAuthenticationServices()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddDsiAuthentication();

        DsiAuthenticationExtensions.AddDsiAuthentication(services, configuration);

        Assert.IsTrue(
            services.Any(descriptor => descriptor.ServiceType == typeof(IAuthenticationService))
        );
    }

    [TestMethod]
    public void AddDsiAuthentication_ConfiguresAuthenticationCookie()
    {
        var services = new ServiceCollection();
        var configuration = CreateFakeConfigurationForAddDsiAuthentication();

        DsiAuthenticationExtensions.AddDsiAuthentication(services, configuration);

        var provider = services.BuildServiceProvider();

        var optionsAccessor = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
        var options = optionsAccessor.Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.AreEqual("Auth", options.Cookie.Name);
        Assert.AreEqual(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.AreEqual(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.IsTrue(options.Cookie.HttpOnly);
        Assert.IsTrue(options.Cookie.IsEssential);
        Assert.AreEqual(25, options.ExpireTimeSpan.TotalMinutes);
    }

    #endregion
}
