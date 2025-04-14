using AutoMapper;
using Dfe.SignIn.SelectOrganisation.Web.MappingProfiles;

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
