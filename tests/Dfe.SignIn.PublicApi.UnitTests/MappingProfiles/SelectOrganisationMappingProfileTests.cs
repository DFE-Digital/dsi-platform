using AutoMapper;
using Dfe.SignIn.PublicApi.MappingProfiles;

namespace Dfe.SignIn.PublicApi.UnitTests.MappingProfiles;

[TestClass]
public sealed class SelectOrganisationMappingProfileTests
{
    [TestMethod]
    public void MappingConfigurationIsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SelectOrganisationMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}
