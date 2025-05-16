using AutoMapper;
using Dfe.SignIn.PublicApi.MappingProfiles;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Users;

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
