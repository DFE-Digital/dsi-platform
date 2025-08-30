using AutoMapper;
using AutoMapper.Internal;
using Dfe.SignIn.PublicApi.Configuration;
using Dfe.SignIn.PublicApi.MappingProfiles;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.UnitTests.Configuration;

[TestClass]
public sealed class AutoMapperExtensionTests
{
    #region SetupAutoMapper(IServiceCollection)

    [TestMethod]
    public void SetupAutoMapper_Throws_WhenServicesArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => AutoMapperExtensions.SetupAutoMapper(
                services: null!
            ));
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
        CollectionAssert.Contains(profileNames, typeof(SelectOrganisationMappingProfile).FullName);
        CollectionAssert.Contains(profileNames, typeof(OrganisationDetailsMappingProfile).FullName);
    }

    #endregion
}
