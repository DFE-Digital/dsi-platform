using AutoMapper;

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
