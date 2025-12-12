using Dfe.SignIn.WebFramework.Mvc.Configuration;
using Dfe.SignIn.WebFramework.Mvc.Filters;
using Dfe.SignIn.WebFramework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Configuration;

[TestClass]
public sealed class MvcExtensionsTests
{
    [TestMethod]
    public void AddDsiMvcExtensions_Throws_WhenBuilderIsNull()
    {
        IMvcBuilder? builder = null;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => builder!.AddDsiMvcExtensions());
    }

    [TestMethod]
    public void AddDsiMvcExtensions_HasExpectedServices()
    {
        var services = new ServiceCollection();
        var builder = services.AddControllers();

        builder.AddDsiMvcExtensions();

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<MvcOptions>>().Value;

        Assert.IsTrue(options.Filters.Any(filter
            => filter is TypeFilterAttribute tfa && tfa.ImplementationType == typeof(RequestBodySizeLimitFilter)
        ));
        Assert.IsTrue(options.ModelBinderProviders.Any(provider
            => provider is TrimStringModelBinderProvider
        ));
    }

    [TestMethod]
    public void AddDsiMvcExtensions_SetsTempDataCookie()
    {
        var services = new ServiceCollection();
        var builder = services.AddControllers();

        builder.AddDsiMvcExtensions();

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<CookieTempDataProviderOptions>>().Value;

        Assert.IsNotNull(options);
        Assert.IsNotNull(options.Cookie);
        Assert.AreEqual("dsi-temp-data", options.Cookie.Name);
        Assert.IsTrue(options.Cookie.HttpOnly);
        Assert.AreEqual(CookieSecurePolicy.SameAsRequest, options.Cookie.SecurePolicy);
    }
}
