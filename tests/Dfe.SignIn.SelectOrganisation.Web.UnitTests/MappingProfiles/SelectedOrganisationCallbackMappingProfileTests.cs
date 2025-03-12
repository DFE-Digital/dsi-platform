using AutoMapper;

namespace Dfe.SignIn.SelectOrganisation.Web.UnitTests.MappingProfiles;

[TestClass]
public sealed class SelectedOrganisationCallbackMappingProfileTests
{
    [TestMethod]
    public void MappingConfigurationIsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SelectedOrganisationCallbackMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}
