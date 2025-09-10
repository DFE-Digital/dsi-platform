
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

    #region Mapping: OrganisationByIdDto -> Organisation

    private static readonly OrganisationByIdDto FakeOrganisationByIdDto = new() {
        Id = new Guid("65916f8e-8704-4a73-b8ed-0c8daa11e6d2"),
        Status = 1,
        Name = "Example organisation",
        Category = "002",
    };

    [DataRow(1, OrganisationStatus.Open)]
    [DataRow(9, OrganisationStatus.CreatedInError)]
    [TestMethod]
    public void OrganisationByIdDto_to_Organisation_MapsStatus(
        int id, OrganisationStatus expectedStatus)
    {
        var organisation = (FakeOrganisationByIdDto with {
            Status = id,
        }).MapToOrganisation();

        Assert.AreEqual(expectedStatus, organisation.Status);
    }

    [DataRow("002", OrganisationCategory.LocalAuthority)]
    [DataRow("052", OrganisationCategory.BillingAuthority)]
    [TestMethod]
    public void OrganisationByIdDto_to_Organisation_MapsCategory(
        string id, OrganisationCategory expectedCategory)
    {
        var organisation = (FakeOrganisationByIdDto with {
            Category = id,
        }).MapToOrganisation();

        Assert.AreEqual(expectedCategory, organisation.Category);
    }

    [DataRow("01", EstablishmentType.CommunitySchool)]
    [DataRow("34", EstablishmentType.AcademyConverter)]
    [TestMethod]
    public void OrganisationByIdDto_to_Organisation_MapsEstablishmentType(
        string id, EstablishmentType expectedEstablishmentType)
    {
        var organisation = (FakeOrganisationByIdDto with {
            Category = OrganisationConstants.CategoryId_Establishment,
            EstablishmentType = id,
        }).MapToOrganisation();

        Assert.AreEqual(expectedEstablishmentType, organisation.EstablishmentType);
    }

    [TestMethod]
    public void OrganisationByIdDto_to_Organisation_EstablishmentTypeIsNull_WhenCategoryIsNotAnEstablishment()
    {
        var organisation = (FakeOrganisationByIdDto with {
            Category = "002",
            EstablishmentType = "01",
        }).MapToOrganisation();

        Assert.IsNull(organisation.EstablishmentType);
    }

    [TestMethod]
    public void OrganisationByIdDto_to_Organisation_ValidateMapping()
    {
        var organisation = (FakeOrganisationByIdDto with {
            Id = Guid.Parse("65916f8e-8704-4a73-b8ed-0c8daa11e6d2"),
            Category = "002",
            Name = "Fake Org",
            LegalName = "Fake Org Legal",
            EstablishmentType = null,
            EstablishmentNumber = "12345",
            Urn = "URN123",
            Uid = "UID123",
            Upin = "UPIN123",
            Ukprn = "UKPRN123",
            ClosedOn = DateOnly.FromDateTime(DateTime.Today),
            Address = "123 Fake Street",
            SourceSystem = "GIAS",
            ProviderTypeName = "School",
            ProviderTypeCode = 10,
            GIASProviderType = "Academy",
            PIMSProviderType = "Trust",
            PIMSProviderTypeCode = 20,
            PIMSStatusName = "Open",
            PIMSStatus = 1,
            GIASStatusName = "Open",
            GIASStatus = 2,
            MasterProviderStatusName = "Approved",
            MasterProviderStatusCode = 99,
            OpenedOn = "2020-01-01",
            DistrictAdministrativeName = "District X",
            DistrictAdministrativeCode = "D123",
            IsOnAPAR = "Yes"
        }).MapToOrganisation();

        Assert.AreEqual(Guid.Parse("65916f8e-8704-4a73-b8ed-0c8daa11e6d2"), organisation.Id);
        Assert.AreEqual("Fake Org", organisation.Name);
        Assert.AreEqual("Fake Org Legal", organisation.LegalName);
        Assert.AreEqual("12345", organisation.EstablishmentNumber);
        Assert.AreEqual("URN123", organisation.Urn);
        Assert.AreEqual("UID123", organisation.Uid);
        Assert.AreEqual("UPIN123", organisation.Upin);
        Assert.AreEqual("UKPRN123", organisation.Ukprn);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.Today), organisation.ClosedOn);
        Assert.AreEqual("123 Fake Street", organisation.Address);
        Assert.AreEqual("GIAS", organisation.SourceSystem);
        Assert.AreEqual("School", organisation.ProviderTypeName);
        Assert.AreEqual(10, organisation.ProviderTypeCode);
        Assert.AreEqual("Academy", organisation.GIASProviderType);
        Assert.AreEqual("Trust", organisation.PIMSProviderType);
        Assert.AreEqual(20, organisation.PIMSProviderTypeCode);
        Assert.AreEqual("Open", organisation.PIMSStatusName);
        Assert.AreEqual(1, organisation.PIMSStatus);
        Assert.AreEqual("Open", organisation.GIASStatusName);
        Assert.AreEqual(2, organisation.GIASStatus);
        Assert.AreEqual("Approved", organisation.MasterProviderStatusName);
        Assert.AreEqual(99, organisation.MasterProviderStatusCode);
        Assert.AreEqual("2020-01-01", organisation.OpenedOn);
        Assert.AreEqual("District X", organisation.DistrictAdministrativeName);
        Assert.AreEqual("D123", organisation.DistrictAdministrativeCode);
        Assert.AreEqual("Yes", organisation.IsOnAPAR);
    }

    #endregion
}
