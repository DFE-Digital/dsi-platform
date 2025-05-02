using AutoMapper;
using Dfe.SignIn.Web.SelectOrganisation.MappingProfiles;

namespace Dfe.SignIn.Web.SelectOrganisation.UnitTests.MappingProfiles;

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
