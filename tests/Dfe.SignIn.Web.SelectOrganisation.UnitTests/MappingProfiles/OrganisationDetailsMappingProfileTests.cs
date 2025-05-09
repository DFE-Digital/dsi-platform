using AutoMapper;
using Dfe.SignIn.Web.SelectOrganisation.MappingProfiles;

namespace Dfe.SignIn.Web.SelectOrganisation.UnitTests.MappingProfiles;

[TestClass]
public sealed class OrganisationDetailsMappingProfileTests
{
    [TestMethod]
    public void MappingConfigurationIsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrganisationDetailsMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}
