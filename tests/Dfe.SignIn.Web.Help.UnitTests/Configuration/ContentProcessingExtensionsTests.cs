using Dfe.SignIn.Web.Help.Configuration;
using Dfe.SignIn.Web.Help.Content;
using Dfe.SignIn.Web.Help.Content.Processing;
using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Web.Help.UnitTests.Configuration;

[TestClass]
public sealed class ContentProcessingExtensionsTests
{
    #region SetupContentProcessing(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupContentProcessing_Throws_WhenServicesArgumentIsNull()
    {
        ContentProcessingExtensions.SetupContentProcessing(
            services: null!
        );
    }

    [TestMethod]
    public void SetupContentProcessing_AddsTopicProcessingServices()
    {
        var services = new ServiceCollection();

        services.SetupContentProcessing();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ITopicMarkdownProcessor)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ITopicFileReader)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ITopicFilePipeline)
            )
        );
        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ServiceType == typeof(ITopicIndexAccessor)
            )
        );
    }

    [TestMethod]
    public async Task SetupContentProcessing_AddsExpectedVariableSubstitutions()
    {
        var services = new ServiceCollection();

        services
            .Configure<PlatformOptions>(options => {
                options.HelpUrl = new Uri("https://help.localhost");
                options.ManageUrl = new Uri("https://manage.localhost");
                options.ProfileUrl = new Uri("https://profile.localhost");
                options.ServicesUrl = new Uri("https://services.localhost");
                options.SupportUrl = new Uri("https://support.localhost");
            })
            .SetupContentProcessing();

        var provider = services.BuildServiceProvider();

        var variableSubstitutionTopicPreProcessor = provider.GetServices<ITopicPreProcessor>()
            .FirstOrDefault(preProcessor => preProcessor is VariableSubstitutionTopicPreProcessor)
            as VariableSubstitutionTopicPreProcessor;

        Assert.IsNotNull(variableSubstitutionTopicPreProcessor);

        string resolvedContent = await variableSubstitutionTopicPreProcessor.ProcessAsync("/", @"
            help = ${{ HELP_URL }}
            manage = ${{ MANAGE_URL }}
            profile = ${{ PROFILE_URL }}
            services = ${{ SERVICES_URL }}
            support = ${{ SUPPORT_URL }}
        ");

        string expectedContent = @"
            help = https://help.localhost/
            manage = https://manage.localhost/
            profile = https://profile.localhost/
            services = https://services.localhost/
            support = https://support.localhost/
        ";

        Assert.AreEqual(expectedContent, resolvedContent);
    }

    #endregion
}
