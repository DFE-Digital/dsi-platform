using Dfe.SignIn.Core.Public;
using Dfe.SignIn.NodeApi.Client.Organisations;
using Dfe.SignIn.NodeApi.Client.Organisations.Models;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.MappingProfile;

[TestClass]
public sealed class OrganisationProfileTests
{
    #region Mapping: UserOrganisationDto -> Organisation

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
    [TestMethod]
    public void UserOrganisationDto_to_Organisation_MapsStatus(
        int id, string name, string tagColor, OrganisationStatus expectedStatus)
    {
        var organisation = (FakeUserOrganisationDto with {
            Status = new StatusDto {
                Id = id,
                Name = name,
                TagColor = tagColor,
            },
        }).MapToOrganisation();

        Assert.AreEqual(expectedStatus, organisation.Status);
    }

    [DataRow("002", "Local Authority", OrganisationCategory.LocalAuthority)]
    [DataRow("052", "Billing Authority", OrganisationCategory.BillingAuthority)]
    [TestMethod]
    public void UserOrganisationDto_to_Organisation_MapsCategory(
        string id, string name, OrganisationCategory expectedCategory)
    {
        var organisation = (FakeUserOrganisationDto with {
            Category = new CategoryDto {
                Id = id,
                Name = name,
            },
        }).MapToOrganisation();

        Assert.AreEqual(expectedCategory, organisation.Category);
    }

    [DataRow("01", "Community School", EstablishmentType.CommunitySchool)]
    [DataRow("34", "Academy Converter", EstablishmentType.AcademyConverter)]
    [TestMethod]
    public void UserOrganisationDto_to_Organisation_MapsEstablishmentType(
        string id, string name, EstablishmentType expectedEstablishmentType)
    {
        var organisation = (FakeUserOrganisationDto with {
            Category = new CategoryDto {
                Id = OrganisationConstants.CategoryId_Establishment,
                Name = "Establishment",
            },
            EstablishmentType = new EstablishmentTypeDto {
                Id = id,
                Name = name,
            },
        }).MapToOrganisation();

        Assert.AreEqual(expectedEstablishmentType, organisation.EstablishmentType);
    }

    [TestMethod]
    public void UserOrganisationDto_to_Organisation_EstablishmentTypeIsNull_WhenCategoryIsNotAnEstablishment()
    {
        var organisation = (FakeUserOrganisationDto with {
            Category = new CategoryDto {
                Id = "002",
                Name = "Local Authority",
            },
            EstablishmentType = new EstablishmentTypeDto {
                Id = "01",
                Name = "Community School",
            },
        }).MapToOrganisation();

        Assert.IsNull(organisation.EstablishmentType);
    }

    #endregion
}
