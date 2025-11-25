using Dfe.SignIn.WebFramework.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.UnitTests.Filters;

[TestClass]
public sealed class RequestBodySizeLimitExtensionsTests
{
    [TestMethod]
    public void AddRequestBodySizeLimitFilter_Throws_WhenBuilderIsNull()
    {
        IMvcBuilder? builder = null;

        Assert.ThrowsExactly<ArgumentNullException>(()
            => builder!.AddRequestBodySizeLimitFilter());
    }

    [TestMethod]
    public void AddRequestBodySizeLimitFilter_HasExpectedServices()
    {
        var services = new ServiceCollection();
        var builder = services.AddControllers();

        builder.AddRequestBodySizeLimitFilter();

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<MvcOptions>>().Value;

        Assert.IsTrue(
            options.Filters.Any(filter =>
                filter is TypeFilterAttribute tfa && tfa.ImplementationType == typeof(RequestBodySizeLimitFilter)
            )
        );
    }
}
