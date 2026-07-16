using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class GetOrganisationsAssociatedWithUserUserCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        var autoMocker = new AutoMocker();
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var orgCtx = new DbOrganisationsContext(options);
        autoMocker.Use(orgCtx);

        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetOrganisationsAssociatedWithUserRequest,
            GetOrganisationsAssociatedWithUserUseCase
        >(autoMocker);
    }

    private static async Task<DbOrganisationsContext> SetupFakeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<DbOrganisationsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        /* var organisations = organisationDbContext.UserOrganisationRequests
            .Where(x => x.UserId == context.Request.UserId)
            .Include(x => x.Organisation)
                .ThenInclude(x => x.Associations)
                .ThenInclude(x => x.AssociatedOrganisation)
            .ToList();
*/
        var ctx = new DbOrganisationsContext(options);

        var localAuthority = new OrganisationEntity {
            Id = Guid.Parse("4be73e7e-e3fc-406a-a336-1504f498becb"),
            Name = "Test LA Organisation",
            Status = (int)OrganisationStatus.Open,
            Category = "002",
        };

        var organisation1 = new OrganisationEntity {
            Id = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
            Name = "Test Organisation 1",
            Status = (int)OrganisationStatus.Open,
            Category = "002"
        };

        organisation1.Associations.Add(new OrganisationAssociationEntity {
            LinkType = "LA",
            AssociatedOrganisation = localAuthority,
            Organisation = organisation1
        });

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            Organisation = organisation1,
            UserId = Guid.Parse("a8a5453a-3547-40ba-9a67-9a67e91101c8"),
            RoleId = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = 1
        });

        ctx.Organisations.Add(new OrganisationEntity {
            Id = Guid.Parse("f9fb271e-2f7c-439c-9c69-332d3281b2d1"),
            Name = "Test Organisation 2",
            Status = (int)OrganisationStatus.Closed,
            Category = "002",
        });

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            OrganisationId = Guid.Parse("f9fb271e-2f7c-439c-9c69-332d3281b2d1"),
            UserId = Guid.Parse("a8a5453a-3547-40ba-9a67-9a67e91101c8"),
            RoleId = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = 1
        });

        ctx.Organisations.Add(new OrganisationEntity {
            Id = Guid.Parse("bc4ab66f-c33a-4aa0-b602-4d40b66f6923"),
            Name = "Not Found",
            Status = (int)OrganisationStatus.Closed,
            Category = "002",
        });

        ctx.UserOrganisations.Add(new UserOrganisationEntity {
            OrganisationId = Guid.Parse("bc4ab66f-c33a-4aa0-b602-4d40b66f6923"),
            UserId = Guid.Parse("5ea4c9c8-e737-41aa-8d70-87328f7bfd26"),
            RoleId = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Status = 1
        });

        await ctx.SaveChangesAsync();

        return ctx;
    }

    [TestMethod]
    public async Task ReturnsExpectedCollection()
    {
        var orgCtx = await SetupFakeDatabaseAsync();
        GetOrganisationsAssociatedWithUserUseCase interactor = new(orgCtx);

        var response = await interactor.InvokeAsync(
            new GetOrganisationsAssociatedWithUserRequest {
                UserId = Guid.Parse("a8a5453a-3547-40ba-9a67-9a67e91101c8")
            }
        );

        var expectedResponse = new GetOrganisationsAssociatedWithUserResponse {
            Organisations = [
                new Organisation {
                    Id = Guid.Parse("d289bd61-06c5-4fcf-b0f1-7509bc3570f4"),
                    Name = "Test Organisation 1",
                    Status = OrganisationStatus.Open,
                    Category = OrganisationCategory.LocalAuthority,
                    CategoryId = "002",
                    LocalAuthority = new LocalAuthority(Guid.Parse("4be73e7e-e3fc-406a-a336-1504f498becb"), "Test LA Organisation", null)
                },
                new Organisation {
                    Id = Guid.Parse("f9fb271e-2f7c-439c-9c69-332d3281b2d1"),
                    Name = "Test Organisation 2",
                    Status = OrganisationStatus.Closed,
                    Category = OrganisationCategory.LocalAuthority,
                    CategoryId = "002"
                }
            ]
        };

        CollectionAssert.AreEqual(
            expectedResponse.Organisations.ToArray(),
            response.Organisations.ToArray()
        );
    }
}
