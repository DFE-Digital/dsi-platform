using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Configuration;

[TestClass]
public class DsiAntiforgeryCookieExtensionsTests
{
    [TestMethod]
    public void ConfigureDsiAntiforgeryCookie_AddsAntiforgeryWithCorrectOptions()
    {
        var services = new ServiceCollection();
        services.ConfigureDsiAntiforgeryCookie();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AntiforgeryOptions>>().Value;

        Assert.AreEqual("dsi-antiforgery", options.Cookie.Name);
        Assert.IsTrue(options.Cookie.HttpOnly);
        Assert.IsTrue(options.Cookie.IsEssential);
        Assert.AreEqual(CookieSecurePolicy.SameAsRequest, options.Cookie.SecurePolicy);
        Assert.AreEqual(SameSiteMode.Strict, options.Cookie.SameSite);
    }

    [TestMethod]
    public void ConfigureDsiAntiforgeryCookie_ThrowsIfServicesNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => {
            IServiceCollection services = null!;
            DsiAntiforgeryCookieExtensions.ConfigureDsiAntiforgeryCookie(services);
        });
    }
}
