using Dfe.SignIn.WebFramework.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Dfe.SignIn.WebFramework.UnitTests.Extensions;

[TestClass]
public sealed class EnvironmentExtensionsTests
{
    [TestMethod]
    public void IsLocal_ReturnsTrue_WhenEnvironmentIsLocal()
    {
        var environment = new TestWebHostEnvironment { EnvironmentName = "Local" };

        Assert.IsTrue(environment.IsLocal());
    }

    [TestMethod]
    public void IsLocal_ReturnsFalse_WhenEnvironmentIsNotLocal()
    {
        var environment = new TestWebHostEnvironment { EnvironmentName = "Dev" };

        Assert.IsFalse(environment.IsLocal());
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
    }
}
