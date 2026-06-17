using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.UseCases.Applications;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Applications;

[TestClass]
public sealed class GetApplicationRolesUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetApplicationRolesRequest,
            GetApplicationRolesUseCase
        >();
    }

    private static readonly Guid AppAId = Guid.Parse("01e876a1-a06d-4945-bbe0-599a3d73dd11");
    private static readonly Guid AppBId = Guid.Parse("33510512-33f3-491c-86ca-e036726584d0");
    private static readonly Guid AppCId = Guid.Parse("f8da743c-eeb7-4be4-8045-54fba2e13419");

    private static async Task SetupFakeDatabaseAsync(AutoMocker autoMocker)
    {
        var ctx = autoMocker.UseInMemoryOrganisationsDb();

        ctx.Services.Add(new ServiceEntity {
            Id = AppAId,
            Name = "Fake Application A",
            ClientId = "fake-app-a",
            ClientSecret = "fake-client-secret-a",
            Description = "The first fake example application.",
            ServiceHome = "https://fake-application-a.localhost",
            IsExternalService = false,
            IsHiddenService = false,
            IsIdOnlyService = false,
        });

        ctx.Services.Add(new ServiceEntity {
            Id = AppBId,
            Name = "Fake Application B",
            ClientId = "fake-app-b",
            ClientSecret = "fake-client-secret-b",
            Description = "The second fake example application.",
            ServiceHome = "https://fake-application-b.localhost",
            IsExternalService = true,
            IsHiddenService = true,
            IsIdOnlyService = true,
        });

        ctx.Services.Add(new ServiceEntity {
            Id = AppCId,
            Name = "Fake Application C",
            ClientId = "fake-app-c",
            ClientSecret = "fake-client-secret-c",
            Description = "The third fake example application.",
            ServiceHome = null,
            IsExternalService = true,
            IsHiddenService = false,
            IsIdOnlyService = false,
        });

        // Add roles for AppA
        ctx.Roles.Add(new RoleEntity {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Name = "Role A1",
            Code = "A1",
            NumericId = 1,
            Status = 1,
            ApplicationId = AppAId
        });
        ctx.Roles.Add(new RoleEntity {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            Name = "Role A2",
            Code = "A2",
            NumericId = 2,
            Status = 2,
            ApplicationId = AppAId
        });
        // Add a role with a parent for AppA
        ctx.Roles.Add(new RoleEntity {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            Name = "Role A2 Child",
            Code = "A2C",
            NumericId = 3,
            Status = 1,
            ApplicationId = AppAId,
            ParentId = Guid.Parse("10000000-0000-0000-0000-000000000002")
        });

        // Add roles for AppB
        ctx.Roles.Add(new RoleEntity {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            Name = "Role B1",
            Code = "B1",
            NumericId = 10,
            Status = 1,
            ApplicationId = AppBId
        });

        // AppC has no roles

        await ctx.SaveChangesAsync();
    }

    public static IEnumerable<object[]> GetCasesForReturnsApplicationRoles()
    {
        // AppA: 2 top-level roles, 1 child role
        yield return new object[] {
            AppAId,
            new GetApplicationRolesResponse {
                Roles = [
                    new() {
                        Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                        Name = "Role A1",
                        Code = "A1",
                        NumericId = 1,
                        Status = (ApplicationRoleStatus)1,
                        Parent = null
                    },
                    new() {
                        Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                        Name = "Role A2",
                        Code = "A2",
                        NumericId = 2,
                        Status = (ApplicationRoleStatus)2,
                        Parent = null
                    },
                    new() {
                        Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                        Name = "Role A2 Child",
                        Code = "A2C",
                        NumericId = 3,
                        Status = (ApplicationRoleStatus)1,
                        Parent = new ApplicationRole {
                            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                            Name = "Role A2",
                            Code = "A2",
                            NumericId = 2,
                            Status = (ApplicationRoleStatus)2,
                            Parent = null
                        }
                    }
                ]
            }
        };
        // AppB: 1 role
        yield return new object[] {
            AppBId,
            new GetApplicationRolesResponse {
                Roles = [
                    new() {
                        Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                        Name = "Role B1",
                        Code = "B1",
                        NumericId = 10,
                        Status = (ApplicationRoleStatus)1,
                        Parent = null
                    }
                ]
            }
        };
        // AppC: no roles
        yield return new object[] {
            AppCId,
            new GetApplicationRolesResponse {
                Roles = []
            }
        };
    }

    [TestMethod]
    [DynamicData(nameof(GetCasesForReturnsApplicationRoles), DynamicDataSourceType.Method)]
    public async Task ReturnsApplicationRoles(Guid applicationId, GetApplicationRolesResponse expectedResponse)
    {
        var autoMocker = new AutoMocker();
        await SetupFakeDatabaseAsync(autoMocker);

        var interactor = autoMocker.CreateInstance<GetApplicationRolesUseCase>();

        var response = await interactor.InvokeAsync(
            new GetApplicationRolesRequest {
                ApplicationId = applicationId,
            }
        );

        // Compare roles deeply (order matters)
        Assert.AreEqual(expectedResponse.Roles.Count(), response.Roles.Count(), "Role count mismatch");
        for (int i = 0 ; i < expectedResponse.Roles.Count() ; i++) {
            AssertApplicationRoleEqual(expectedResponse.Roles.ElementAt(i), response.Roles.ElementAt(i));
        }
    }

    private static void AssertApplicationRoleEqual(ApplicationRole expected, ApplicationRole actual)
    {
        Assert.AreEqual(expected.Id, actual.Id, "Role Id mismatch");
        Assert.AreEqual(expected.Name, actual.Name, "Role Name mismatch");
        Assert.AreEqual(expected.Code, actual.Code, "Role Code mismatch");
        Assert.AreEqual(expected.NumericId, actual.NumericId, "Role NumericId mismatch");
        Assert.AreEqual(expected.Status, actual.Status, "Role Status mismatch");
        if (expected.Parent == null) {
            Assert.IsNull(actual.Parent, "Expected null parent");
        }
        else {
            AssertApplicationRoleEqual(expected.Parent, actual.Parent!);
        }
    }
}
