
using AutoMapper;

namespace Dfe.SignIn.NodeApiClient.UnitTests.MappingProfile;

[TestClass]
public class OrganisationProfileTests
{
    [TestMethod]
    public void ValidateAutoMapperMappings()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles.OrganisationProfile>());

        configuration.AssertConfigurationIsValid();
    }
}
