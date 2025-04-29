
using AutoMapper;
using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.InternalModels.Organisations;
using Dfe.SignIn.NodeApi.Client.MappingProfiles;
using Dfe.SignIn.NodeApi.Client.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations.Models;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.MappingProfile;

[TestClass]
public sealed class OrganisationProfileTests
{
    private static MapperConfiguration CreateMapperConfiguration()
    {
        return new MapperConfiguration(cfg => cfg.AddProfile<OrganisationProfile>());
    }

    private static IMapper CreateMapper()
    {
        return CreateMapperConfiguration().CreateMapper();
    }

    [TestMethod]
    public void ValidateAutoMapperMappings()
    {
        var configuration = CreateMapperConfiguration();

        configuration.AssertConfigurationIsValid();
    }

    #region Mapping: UserOrganisationDto -> OrganisationModel

    private static readonly UserOrganisationDto FakeUserOrganisationDto = new() {
        Id = new Guid("65916f8e-8704-4a73-b8ed-0c8daa11e6d2"),
        Status = new StatusDto {
            Id = 1,
            Name = "Open",
            TagColor = "green",
        },
        Name = "Example organisation",
        Category = new CategoryDto {
            Id = "002",
            Name = "Local Authority",
        },
    };

    [DataRow(1, "Open", "green", OrganisationStatus.Open)]
    [DataRow(9, "Created in error", "red", OrganisationStatus.CreatedInError)]
    [DataTestMethod]
    public void UserOrganisationDto_to_OrganisationModel_MapsStatus(
        int id, string name, string tagColor, OrganisationStatus expectedStatus)
    {
        var mapper = CreateMapper();

        var organisation = mapper.Map<OrganisationModel>(FakeUserOrganisationDto with {
            Status = new StatusDto {
                Id = id,
                Name = name,
                TagColor = tagColor,
            },
        });

        Assert.AreEqual(expectedStatus, organisation.Status);
    }

    [DataRow("002", "Local Authority", OrganisationCategory.LocalAuthority)]
    [DataRow("052", "Billing Authority", OrganisationCategory.BillingAuthority)]
    [DataTestMethod]
    public void UserOrganisationDto_to_OrganisationModel_MapsCategory(
        string id, string name, OrganisationCategory expectedCategory)
    {
        var mapper = CreateMapper();

        var organisation = mapper.Map<OrganisationModel>(FakeUserOrganisationDto with {
            Category = new CategoryDto {
                Id = id,
                Name = name,
            },
        });

        Assert.AreEqual(expectedCategory, organisation.Category);
    }

    [DataRow("01", "Community School", EstablishmentType.CommunitySchool)]
    [DataRow("34", "Academy Converter", EstablishmentType.AcademyConverter)]
    [DataTestMethod]
    public void UserOrganisationDto_to_OrganisationModel_MapsEstablishmentType(
        string id, string name, EstablishmentType expectedEstablishmentType)
    {
        var mapper = CreateMapper();

        var organisation = mapper.Map<OrganisationModel>(FakeUserOrganisationDto with {
            Category = new CategoryDto {
                Id = OrganisationConstants.CategoryId_Establishment,
                Name = "Establishment",
            },
            EstablishmentType = new EstablishmentTypeDto {
                Id = id,
                Name = name,
            },
        });

        Assert.AreEqual(expectedEstablishmentType, organisation.EstablishmentType);
    }

    [TestMethod]
    public void UserOrganisationDto_to_OrganisationModel_EstablishmentTypeIsNull_WhenCategoryIsNotAnEstablishment()
    {
        var mapper = CreateMapper();

        var organisation = mapper.Map<OrganisationModel>(FakeUserOrganisationDto with {
            Category = new CategoryDto {
                Id = "002",
                Name = "Local Authority",
            },
            EstablishmentType = new EstablishmentTypeDto {
                Id = "01",
                Name = "Community School",
            },
        });

        Assert.IsNull(organisation.EstablishmentType);
    }

    #endregion

    #region Mapping: OrganisationByIdDto -> OrganisationModel

    private static readonly OrganisationByIdDto FakeOrganisationByIdDto = new() {
        Id = new Guid("65916f8e-8704-4a73-b8ed-0c8daa11e6d2"),
        Status = 1,
        Name = "Example organisation",
        Category = "002",
    };

    [DataRow(1, OrganisationStatus.Open)]
    [DataRow(9, OrganisationStatus.CreatedInError)]
    [DataTestMethod]
    public void OrganisationByIdDto_to_OrganisationModel_MapsStatus(
        int id, OrganisationStatus expectedStatus)
    {
        var mapper = CreateMapper();

        var organisation = mapper.Map<OrganisationModel>(FakeOrganisationByIdDto with {
            Status = id,
        });

        Assert.AreEqual(expectedStatus, organisation.Status);
    }

    [DataRow("002", OrganisationCategory.LocalAuthority)]
    [DataRow("052", OrganisationCategory.BillingAuthority)]
    [DataTestMethod]
    public void OrganisationByIdDto_to_OrganisationModel_MapsCategory(
        string id, OrganisationCategory expectedCategory)
    {
        var mapper = CreateMapper();

        var organisation = mapper.Map<OrganisationModel>(FakeOrganisationByIdDto with {
            Category = id,
        });

        Assert.AreEqual(expectedCategory, organisation.Category);
    }

    [DataRow("01", EstablishmentType.CommunitySchool)]
    [DataRow("34", EstablishmentType.AcademyConverter)]
    [DataTestMethod]
    public void OrganisationByIdDto_to_OrganisationModel_MapsEstablishmentType(
        string id, EstablishmentType expectedEstablishmentType)
    {
        var mapper = CreateMapper();

        var organisation = mapper.Map<OrganisationModel>(FakeOrganisationByIdDto with {
            Category = OrganisationConstants.CategoryId_Establishment,
            EstablishmentType = id,
        });

        Assert.AreEqual(expectedEstablishmentType, organisation.EstablishmentType);
    }

    [TestMethod]
    public void OrganisationByIdDto_to_OrganisationModel_EstablishmentTypeIsNull_WhenCategoryIsNotAnEstablishment()
    {
        var mapper = CreateMapper();

        var organisation = mapper.Map<OrganisationModel>(FakeOrganisationByIdDto with {
            Category = "002",
            EstablishmentType = "01",
        });

        Assert.IsNull(organisation.EstablishmentType);
    }

    #endregion
}
