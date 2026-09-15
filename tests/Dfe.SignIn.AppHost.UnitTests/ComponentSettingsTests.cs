using Microsoft.Extensions.Configuration;

namespace Dfe.SignIn.AppHost.UnitTests;

[TestClass]
public sealed class ComponentSettingsTests
{
    [TestMethod]
    public void Defaults_EnableDotNetAndNodeApps_DisableTlsProxy()
    {
        var settings = new ComponentSettings();

        Assert.IsTrue(settings.DotNet.Help);
        Assert.IsTrue(settings.DotNet.Profile);
        Assert.IsTrue(settings.DotNet.PublicApi);
        Assert.IsTrue(settings.Node.Oidc);
        Assert.IsTrue(settings.Node.Interactions);
        Assert.IsTrue(settings.Node.Services);
        Assert.IsFalse(settings.Tools.TlsProxy);
    }

    [TestMethod]
    public void Bind_MapsNestedComponentToggles()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Components:DotNet:Help"] = "false",
                ["Components:DotNet:Profile"] = "true",
                ["Components:DotNet:PublicApi"] = "false",
                ["Components:Node:Oidc"] = "true",
                ["Components:Node:Interactions"] = "false",
                ["Components:Node:Services"] = "true",
                ["Components:Tools:TlsProxy"] = "true",
            })
            .Build();

        var settings = configuration.GetSection(ComponentSettings.SectionName).Get<ComponentSettings>();

        Assert.IsNotNull(settings);
        Assert.IsFalse(settings.DotNet.Help);
        Assert.IsTrue(settings.DotNet.Profile);
        Assert.IsFalse(settings.DotNet.PublicApi);
        Assert.IsTrue(settings.Node.Oidc);
        Assert.IsFalse(settings.Node.Interactions);
        Assert.IsTrue(settings.Node.Services);
        Assert.IsTrue(settings.Tools.TlsProxy);
    }
}
