using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.PublicApi.MappingExtensions;

namespace Dfe.SignIn.PublicApi.UnitTests.MappingExtensions;

[TestClass]
public class GetUserOrganisationServiceMappingTests
{
    private static GetUserOrganisationService CreateModel(
        Guid userId,
        Guid orgId,
        string? serviceName,
        string? roleName,
        string? roleCode)
    {
        return new GetUserOrganisationService {
            UserId = userId,
            UserStatus = 1,
            Email = "test@test.com",
            FamilyName = "Doe",
            GivenName = "John",

            OrganisationId = orgId,
            OrganisationName = "Org 1",
            CategoryId = OrganisationCategory.Establishment.ToString(), // valid enum input (string → int)
            StatusId = 1,

            ServiceName = serviceName,
            ServiceDescription = "desc",

            RoleName = roleName,
            RoleCode = roleCode,

            OrgRoleId = OrganisationRoles.Approver.Id
        };
    }

    [TestMethod]
    public void ToUserDtos_GroupsByUser_ReturnsSingleUser()
    {
        var userId = Guid.NewGuid();

        var models = new[]
        {
            CreateModel(userId, Guid.NewGuid(), "ServiceA", "Role1", "R1"),
            CreateModel(userId, Guid.NewGuid(), "ServiceB", "Role2", "R2")
        };

        var result = models.ToUserDtos().ToList();

        Assert.HasCount(1, result);
        Assert.AreEqual(userId, result[0].UserId);
    }

    [TestMethod]
    public void ToUserDtos_MapsUserFields_Correctly()
    {
        var userId = Guid.NewGuid();

        var model = CreateModel(userId, Guid.NewGuid(), "ServiceA", "Role1", "R1");

        var result = new[] { model }.ToUserDtos().Single();

        Assert.AreEqual(model.Email, result.Email);
        Assert.AreEqual(model.FamilyName, result.FamilyName);
        Assert.AreEqual(model.GivenName, result.GivenName);
        Assert.AreEqual(model.UserStatus, result.UserStatus);
    }

    [TestMethod]
    public void ToOrganisationDtos_GroupsByOrganisation()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var models = new[]
        {
            CreateModel(userId, orgId, "ServiceA", "Role1", "R1"),
            CreateModel(userId, orgId, "ServiceB", "Role2", "R2")
        };

        var result = models.ToUserDtos().Single();

        Assert.AreEqual(1, result.Organisations.Count());
    }

    [TestMethod]
    public void ToOrganisationDtos_MapsOrganisationFields()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var model = CreateModel(userId, orgId, "ServiceA", "Role1", "R1");

        var org = new[] { model }
            .ToUserDtos()
            .Single()
            .Organisations
            .Single();

        Assert.AreEqual(model.OrganisationId, org.Id);
        Assert.AreEqual(model.OrganisationName, org.Name);
        Assert.AreEqual(model.CategoryId, org.Category?.Id);
        Assert.AreEqual(model.StatusId, org.Status?.Id);
    }

    [TestMethod]
    public void ToServiceDtos_GroupsByServiceName()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var models = new[]
        {
            CreateModel(userId, orgId, "ServiceA", "Role1", "R1"),
            CreateModel(userId, orgId, "ServiceA", "Role2", "R2")
        };

        var services = models
            .ToUserDtos()
            .Single()
            .Organisations
            .Single()
            .Services;

        Assert.AreEqual(1, services.Count());
    }

    [TestMethod]
    public void ToServiceDtos_OrdersServices_ByName()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var models = new[]
        {
            CreateModel(userId, orgId, "BService", "Role1", "R1"),
            CreateModel(userId, orgId, "AService", "Role1", "R1")
        };

        var services = models
            .ToUserDtos()
            .Single()
            .Organisations
            .Single()
            .Services
            .ToList();

        Assert.AreEqual("AService", services[0].Name);
        Assert.AreEqual("BService", services[1].Name);
    }

    [TestMethod]
    public void ToRoleDtos_RemovesDuplicateRoles()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var models = new[]
        {
            CreateModel(userId, orgId, "ServiceA", "Role1", "R1"),
            CreateModel(userId, orgId, "ServiceA", "Role1", "R1") // duplicate
        };

        var roles = models
            .ToUserDtos()
            .Single()
            .Organisations
            .Single()
            .Services
            .Single()
            .Roles;

        Assert.AreEqual(1, roles.Count());
    }

    [TestMethod]
    public void ToRoleDtos_ExcludesNullRoleNames()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var models = new[]
        {
            CreateModel(userId, orgId, "ServiceA", null, "R1"),
            CreateModel(userId, orgId, "ServiceA", "Role1", "R1")
        };

        var roles = models
            .ToUserDtos()
            .Single()
            .Organisations
            .Single()
            .Services
            .Single()
            .Roles
            .ToList();

        Assert.HasCount(1, roles);
        Assert.AreEqual("Role1", roles[0].Name);
    }

    [TestMethod]
    public void ToOrganisationDtos_ConvertsLegacyIdToString()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var model = CreateModel(userId, orgId, "ServiceA", "Role1", "R1");
        model.LegacyId = 12345;

        var org = new[] { model }
            .ToUserDtos()
            .Single()
            .Organisations
            .Single();

        Assert.AreEqual("12345", org.LegacyId);
    }

    [TestMethod]
    public void ToOrganisationDtos_MapsOrgRoleName_WhenExists()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var model = CreateModel(userId, orgId, "ServiceA", "Role1", "R1");

        var org = new[] { model }
            .ToUserDtos()
            .Single()
            .Organisations
            .Single();

        Assert.AreEqual(org.OrgRoleName, OrganisationRoles.Approver.Name);
    }
}
