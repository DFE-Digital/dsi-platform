using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.UseCases.Organisations;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Organisations;

[TestClass]
public class OrganisationHelpersTests
{
    [TestMethod]
    public void OrganisationFromEntity_MapsBasicProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        var entity = new OrganisationEntity {
            Id = id,
            Name = "Test School",
            LegalName = "Test School Limited",
            Category = "002",
            EstablishmentNumber = "1234",
            Urn = "100001",
            Ukprn = "200001",
            RegionCode = "R1",
            PhaseOfEducation = 1,
            Status = 1
        };

        // Act
        var result = OrganisationHelpers.OrganisationFromEntity(entity);

        // Assert
        Assert.AreEqual(id, result.Id);
        Assert.AreEqual(entity.Name, result.Name);
        Assert.AreEqual(entity.LegalName, result.LegalName);
        Assert.AreEqual(entity.Category, result.CategoryId);
        Assert.AreEqual(entity.EstablishmentNumber, result.EstablishmentNumber);
        Assert.AreEqual(entity.Urn, result.Urn);
        Assert.AreEqual(entity.Ukprn, result.Ukprn);
        Assert.AreEqual(entity.RegionCode, result.RegionCode);
        Assert.AreEqual(entity.PhaseOfEducation, result.PhaseOfEducation);
    }

    [TestMethod]
    public void OrganisationFromEntity_SetsEstablishmentType_WhenCategoryIsEstablishment_AndTypeProvided()
    {
        // Arrange
        var entity = new OrganisationEntity {
            Category = OrganisationHelpers.CategoryId_Establishment,
            Type = "2",
            Status = 1
        };

        // Act
        var result = OrganisationHelpers.OrganisationFromEntity(entity);

        // Assert
        Assert.IsNotNull(result.EstablishmentType);
        Assert.AreEqual(EstablishmentType.VoluntaryAidedSchool,
            result.EstablishmentType);
    }

    [TestMethod]
    public void OrganisationFromEntity_DoesNotSetEstablishmentType_WhenCategoryIsNotEstablishment()
    {
        // Arrange
        var entity = new OrganisationEntity {
            Category = "999",
            Type = nameof(EstablishmentType.SecureUnits),
            Status = 1
        };

        // Act
        var result = OrganisationHelpers.OrganisationFromEntity(entity);

        // Assert
        Assert.IsNull(result.EstablishmentType);
    }

    [TestMethod]
    public void OrganisationFromEntity_DoesNotSetEstablishmentType_WhenTypeIsNull()
    {
        // Arrange
        var entity = new OrganisationEntity {
            Category = OrganisationHelpers.CategoryId_Establishment,
            Type = null,
            Status = 0
        };

        // Act
        var result = OrganisationHelpers.OrganisationFromEntity(entity);

        // Assert
        Assert.IsNull(result.EstablishmentType);
    }

    [TestMethod]
    public void OrganisationFromEntity_MapsLocalAuthority_WhenLaAssociationExists()
    {
        // Arrange
        var laId = Guid.NewGuid();

        var entity = new OrganisationEntity {
            Associations =
            [
                new()
                {
                    LinkType = "LA",
                    AssociatedOrganisation = new OrganisationEntity
                    {
                        Id = laId,
                        Name = "Essex County Council",
                        EstablishmentNumber = "881"
                    }
                }
            ],
            Status = 1,
            Category = OrganisationHelpers.CategoryId_Establishment,
        };

        // Act
        var result = OrganisationHelpers.OrganisationFromEntity(entity);

        // Assert
        Assert.IsNotNull(result.LocalAuthority);
        Assert.AreEqual(laId, result.LocalAuthority.Id);
        Assert.AreEqual("Essex County Council", result.LocalAuthority.Name);
        Assert.AreEqual("881", result.LocalAuthority.Code);
    }

    [TestMethod]
    public void OrganisationFromEntity_ReturnsNullLocalAuthority_WhenNoLaAssociationExists()
    {
        // Arrange
        var unknownId = Guid.NewGuid();

        var entity = new OrganisationEntity {
            Associations =
            [
               new()
                {
                    LinkType = "Unknown",
                    AssociatedOrganisation = new OrganisationEntity
                    {
                        Id = unknownId,
                        Name = "Essex County Council",
                        EstablishmentNumber = "881"
                    }
                }
            ],
            Status = 1,
            Category = OrganisationHelpers.CategoryId_Establishment
        };

        // Act
        var result = OrganisationHelpers.OrganisationFromEntity(entity);

        // Assert
        Assert.IsNull(result.LocalAuthority);
    }

    [TestMethod]
    public void OrganisationFromEntity_ParsesPimsStatus_WhenValueProvided()
    {
        // Arrange
        var entity = new OrganisationEntity {
            PimsStatus = "5",
            Status = 1,
            Category = OrganisationHelpers.CategoryId_Establishment
        };

        // Act
        var result = OrganisationHelpers.OrganisationFromEntity(entity);

        // Assert
        Assert.AreEqual(5, result.PimsStatus);
    }

    [TestMethod]
    public void OrganisationFromEntity_ReturnsNullPimsStatus_WhenValueIsNull()
    {
        // Arrange
        var entity = new OrganisationEntity {
            PimsStatus = null,
            Status = 1,
            Category = OrganisationHelpers.CategoryId_Establishment
        };

        // Act
        var result = OrganisationHelpers.OrganisationFromEntity(entity);

        // Assert
        Assert.IsNull(result.PimsStatus);
    }

    [TestMethod]
    public void OrganisationFromEntity_ThrowsFormatException_WhenPimsStatusIsNotNumeric()
    {
        // Arrange
        var entity = new OrganisationEntity {
            PimsStatus = "ABC",
            Status = 1,
            Category = OrganisationHelpers.CategoryId_Establishment
        };

        // Act / Assert
        Assert.ThrowsExactly<FormatException>(
            () => OrganisationHelpers.OrganisationFromEntity(entity));
    }
}
