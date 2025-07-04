using AutoMapper;
using AutoMapper.Internal;
using Dfe.SignIn.Web.Help.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Web.Help.UnitTests.Configuration;

[TestClass]
public sealed class AutoMapperExtensionTests
{
    #region SetupAutoMapper(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupAutoMapper_Throws_WhenServicesArgumentIsNull()
    {
        AutoMapperExtensions.SetupAutoMapper(
            services: null!
        );
    }

    [TestMethod]
    public void SetupAutoMapper_AddsExpectedProfiles()
    {
        var services = new ServiceCollection();

        services.SetupAutoMapper();

        var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
        var mapper = provider.GetRequiredService<IMapper>();
        var profileNames = mapper.ConfigurationProvider.Internal().Profiles
            .Select(profile => profile.Name)
            .ToArray();
        // CollectionAssert.Contains(profileNames, typeof(ExampleMappingProfile).FullName);
    }

    #endregion
}
