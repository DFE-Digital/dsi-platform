using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.Models.Users.Interactions;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.SelectOrganisation;

[TestClass]
public sealed class FilterOrganisationsForUser_UseCaseTests
{
    private static readonly Guid FakeUserId = new("4785f325-f57f-4a3f-a1e3-5f299ce59d71");

    private static readonly FilterOrganisationsForUserRequest FakeBasicRequest = new() {
        ClientId = "fake-client",
        UserId = FakeUserId,
    };

    private static readonly OrganisationModel FakeOrganisationA = new() {
        Id = new Guid("a2dccb2c-6f1f-41f3-a9e3-84f081cc857c"),
        Name = "Organisation A",
        LegalName = "Legal Name A",
        Status = 1,
    };

    private static readonly OrganisationModel FakeOrganisationB = new() {
        Id = new Guid("5728e48d-c067-400a-a211-8ec2c2e09b38"),
        Name = "Organisation B",
        LegalName = "Legal Name B",
        Status = 1,
    };

    private static readonly OrganisationModel FakeOrganisationC = new() {
        Id = new Guid("69fdbe34-9fb0-4599-8609-d77ce665b433"),
        Name = "Organisation C",
        LegalName = "Legal Name C",
        Status = 1,
    };

    private static readonly IEnumerable<OrganisationModel> FakeOrganisations = [
        FakeOrganisationA, FakeOrganisationB, FakeOrganisationC
    ];

    private static readonly ApplicationModel FakeApplication = new() {
        ClientId = "fake-client",
        Id = new Guid("0302059d-0241-4963-ae2b-d0a00715c8d5"),
        Name = "Fake Application",
        IsExternalService = true,
        IsIdOnlyService = false,
        IsHiddenService = false,
        ServiceHomeUrl = new Uri("https://fake-service.localhost"),
    };

    private static readonly Guid FakeAnotherApplicationId = new("651ad5f5-5398-4598-afc1-af8950d4cf86");

    private static void MockOrganisationsAssociatedWithUser(
        AutoMocker autoMocker,
        OrganisationModel[]? organisations = null)
    {
        autoMocker.GetMock<IInteractor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>>()
            .Setup(x => x.InvokeAsync(It.IsAny<GetOrganisationsAssociatedWithUserRequest>()))
            .ReturnsAsync(new GetOrganisationsAssociatedWithUserResponse {
                Organisations = organisations ?? FakeOrganisations,
            });
    }

    private static void MockClientApplication(
        AutoMocker autoMocker,
        ApplicationModel? application)
    {
        autoMocker.GetMock<IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>>()
            .Setup(x => x.InvokeAsync(
                It.Is<GetApplicationByClientIdRequest>(
                    request => request.ClientId == "fake-client"
                )
            ))
            .ReturnsAsync(new GetApplicationByClientIdResponse {
                Application = application,
            });
    }

    private static void MockApplicationsAssociatedWithUser(
        AutoMocker autoMocker,
        Guid userId,
        UserApplicationMappingModel[]? mappings = null)
    {
        autoMocker.GetMock<IInteractor<GetApplicationsAssociatedWithUserRequest, GetApplicationsAssociatedWithUserResponse>>()
            .Setup(x => x.InvokeAsync(
                It.Is<GetApplicationsAssociatedWithUserRequest>(
                    request => request.UserId == userId
                )
            ))
            .ReturnsAsync(new GetApplicationsAssociatedWithUserResponse {
                UserApplicationMappings = mappings ?? [],
            });
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvokeAsync_Throws_WhenUnexpectedFilterTypeIsSupplied()
    {
        var autoMocker = new AutoMocker();
        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = (OrganisationFilterType)(-1),
            },
        });
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task InvokeAsync_Throws_WhenApplicationIsNotFound()
    {
        var autoMocker = new AutoMocker();
        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        MockOrganisationsAssociatedWithUser(autoMocker);

        // Force "fake-client" to be non-existent.
        MockClientApplication(autoMocker, null);

        await useCase.InvokeAsync(FakeBasicRequest with {
            ClientId = "fake-client",
            Filter = new() {
                Type = OrganisationFilterType.Associated,
            },
        });
    }

    #region Type: OrganisationFilterType.AnyOf

    // TODO: Implement `OrganisationFilterType.AnyOf` tests here...

    #endregion

    #region Type: OrganisationFilterType.Associated with OrganisationFilterAssociation.Auto

    [TestMethod]
    public async Task InvokeAsync_AssociatedWithAuto_ReturnsNone_WhenNoOrganisationsAreAssociatedWithUser()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker, []);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.Auto,
            },
        });

        Assert.AreEqual(0, response.FilteredOrganisations.Count());
    }

    [TestMethod]
    public async Task InvokeAsync_AssociatedWithAuto_ReturnsAllOrganisations_WhenApplicationIsIdOnly()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);
        MockClientApplication(autoMocker, FakeApplication with {
            IsIdOnlyService = true,
        });

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.Auto,
            },
        });

        CollectionAssert.AreEqual(
            FakeOrganisations.ToArray(),
            response.FilteredOrganisations.ToArray()
        );
    }

    [TestMethod]
    public async Task InvokeAsync_AssociatedWithAuto_ReturnsAllOrganisations_WhenApplicationIsRoleBaseAndUserNotAssociatedWithApplication()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);
        MockClientApplication(autoMocker, FakeApplication with {
            IsIdOnlyService = false,
        });
        MockApplicationsAssociatedWithUser(autoMocker, FakeUserId, []);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.Auto,
            },
        });

        CollectionAssert.AreEqual(
            FakeOrganisations.ToArray(),
            response.FilteredOrganisations.ToArray()
        );
    }

    [TestMethod]
    public async Task InvokeAsync_AssociatedWithAuto_ReturnsAllOrganisations_WhenApplicationIsRoleBaseAndUserAssociatedWithDifferentApplication()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);
        MockClientApplication(autoMocker, FakeApplication with {
            IsIdOnlyService = false,
        });
        MockApplicationsAssociatedWithUser(autoMocker, FakeUserId, [
            new() {
                UserId = FakeUserId,
                OrganisationId = FakeOrganisationA.Id,
                ApplicationId = FakeAnotherApplicationId,
                AccessGranted = new DateTime(),
            },
        ]);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.Auto,
            },
        });

        CollectionAssert.AreEqual(
            FakeOrganisations.ToArray(),
            response.FilteredOrganisations.ToArray()
        );
    }

    [TestMethod]
    public async Task InvokeAsync_AssociatedWithAuto_ReturnsAssociatedApplicationOrganisations_WhenApplicationIsRoleBaseAndUserAssociatedWithApplication()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);
        MockClientApplication(autoMocker, FakeApplication with {
            IsIdOnlyService = false,
        });
        MockApplicationsAssociatedWithUser(autoMocker, FakeUserId, [
            new() {
                UserId = FakeUserId,
                OrganisationId = FakeOrganisationA.Id,
                ApplicationId = FakeApplication.Id,
                AccessGranted = new DateTime(),
            },
            new() {
                UserId = FakeUserId,
                OrganisationId = FakeOrganisationC.Id,
                ApplicationId = FakeApplication.Id,
                AccessGranted = new DateTime(),
            },
        ]);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.Auto,
            },
        });

        var expected = new OrganisationModel[] {
            FakeOrganisationA,
            FakeOrganisationC,
        };
        CollectionAssert.AreEqual(expected, response.FilteredOrganisations.ToArray());
    }

    #endregion

    #region Type: OrganisationFilterType.Associated with OrganisationFilterAssociation.AssignedToUser

    [TestMethod]
    public async Task InvokeAsync_AssociatedWithAssignedToUser_ReturnsNone_WhenNoOrganisationsAreAssociatedWithUser()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker, []);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.AssignedToUser,
            },
        });

        Assert.AreEqual(0, response.FilteredOrganisations.Count());
    }

    [TestMethod]
    public async Task InvokeAsync_AssociatedWithAssignedToUser_ReturnsAllOrganisations()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.AssignedToUser,
            },
        });

        CollectionAssert.AreEqual(
            FakeOrganisations.ToArray(),
            response.FilteredOrganisations.ToArray()
        );
    }

    #endregion

    #region Type: OrganisationFilterType.Associated with OrganisationFilterAssociation.AssignedToUserForService

    [TestMethod]
    public async Task InvokeAsync_AssociatedWithAssignedToUserForService_ReturnsNone_WhenNoOrganisationsAreAssociatedWithUser()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker, []);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.AssignedToUserForService,
            },
        });

        Assert.AreEqual(0, response.FilteredOrganisations.Count());
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task InvokeAsync_AssociatedWithAssignedToUserForService_ReturnsNone_WhenApplicationIsNotAssociatedWithApplication(
        bool isIdOnlyService)
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);
        MockClientApplication(autoMocker, FakeApplication with {
            IsIdOnlyService = isIdOnlyService,
        });
        MockApplicationsAssociatedWithUser(autoMocker, FakeUserId, []);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.AssignedToUserForService,
            },
        });

        Assert.AreEqual(0, response.FilteredOrganisations.Count());
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task InvokeAsync_AssociatedWithAssignedToUserForServiceReturnsAssociatedApplicationOrganisations_WhenUserAssociatedWithApplication(
        bool isIdOnlyService)
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);
        MockClientApplication(autoMocker, FakeApplication with {
            IsIdOnlyService = isIdOnlyService,
        });
        MockApplicationsAssociatedWithUser(autoMocker, FakeUserId, [
            new() {
                UserId = FakeUserId,
                OrganisationId = FakeOrganisationA.Id,
                ApplicationId = FakeApplication.Id,
                AccessGranted = new DateTime(),
            },
            new() {
                UserId = FakeUserId,
                OrganisationId = FakeOrganisationC.Id,
                ApplicationId = FakeApplication.Id,
                AccessGranted = new DateTime(),
            },
        ]);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.Associated,
                Association = OrganisationFilterAssociation.AssignedToUserForService,
            },
        });

        var expected = new OrganisationModel[] {
            FakeOrganisationA,
            FakeOrganisationC,
        };
        CollectionAssert.AreEqual(expected, response.FilteredOrganisations.ToArray());
    }

    #endregion

    #region Type: OrganisationFilterType.AssociatedInclude

    [TestMethod]
    public async Task InvokeAsync_AssociatedIncludeWithAssignedToUser_ReturnsOnlyIncludedOrganisations()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.AssociatedInclude,
                Association = OrganisationFilterAssociation.AssignedToUser,
                OrganisationIds = [
                    FakeOrganisationA.Id,
                    FakeOrganisationC.Id,
                ],
            },
        });

        var expected = new OrganisationModel[] {
            FakeOrganisationA,
            FakeOrganisationC,
        };
        CollectionAssert.AreEqual(expected, response.FilteredOrganisations.ToArray());
    }

    [TestMethod]
    public async Task InvokeAsync_AssociatedIncludeWithAssignedToUser_ReturnsNone_WhenNoneIncluded()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.AssociatedInclude,
                Association = OrganisationFilterAssociation.AssignedToUser,
                OrganisationIds = [],
            },
        });

        Assert.AreEqual(0, response.FilteredOrganisations.Count());
    }

    #endregion

    #region Type: OrganisationFilterType.AssociatedExclude

    [TestMethod]
    public async Task InvokeAsync_AssociatedExcludeWithAssignedToUser_ReturnsOnlyIncludedOrganisations()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.AssociatedExclude,
                Association = OrganisationFilterAssociation.AssignedToUser,
                OrganisationIds = [
                    FakeOrganisationA.Id,
                    FakeOrganisationC.Id,
                ],
            },
        });

        var expected = new OrganisationModel[] {
            FakeOrganisationB,
        };
        CollectionAssert.AreEqual(expected, response.FilteredOrganisations.ToArray());
    }

    [TestMethod]
    public async Task InvokeAsync_AssociatedIncludeWithAssignedToUser_ReturnsAll_WhenNoneExcluded()
    {
        var autoMocker = new AutoMocker();
        MockOrganisationsAssociatedWithUser(autoMocker);

        var useCase = autoMocker.CreateInstance<FilterOrganisationsForUser_UseCase>();

        var response = await useCase.InvokeAsync(FakeBasicRequest with {
            Filter = new() {
                Type = OrganisationFilterType.AssociatedExclude,
                Association = OrganisationFilterAssociation.AssignedToUser,
                OrganisationIds = [],
            },
        });

        Assert.AreEqual(3, response.FilteredOrganisations.Count());
    }

    #endregion
}
