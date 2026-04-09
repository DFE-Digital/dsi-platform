using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.UseCases.PublicApi;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.PublicApi;

[TestClass]
public sealed class GetUserAccessToServiceUseCaseTests
{
    private static readonly Guid UserId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001");
    private static readonly Guid ServiceId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000002");
    private static readonly Guid OrganisationId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000003");

    private static readonly GetUserAccessToServiceRequest ValidRequest = new() {
        UserId = UserId,
        ServiceId = ServiceId,
        OrganisationId = OrganisationId,
    };

    private static readonly UserServiceRole[] FakeRoles = [
        new() { Id = Guid.Parse("b1b2b3b4-0000-0000-0000-000000000001"), Name = "Role A", Code = "role-a", NumericId = 1 }
    ];

    private static readonly UserServiceIdentifier[] FakeIdentifiers = [
        new() { Key = "externalId", Value = "EXT-001" }
    ];

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserAccessToServiceRequest,
            GetUserAccessToServiceUseCase
        >();
    }

    private static AutoMocker SetupWithAccess()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetUserServiceAccessRequest>(
            new GetUserServiceAccessResponse {
                Access = new UserServiceAccess {
                    UserId = UserId,
                    ServiceId = ServiceId,
                    OrganisationId = OrganisationId,
                    Roles = FakeRoles,
                    Identifiers = FakeIdentifiers,
                }
            }
        );

        autoMocker.MockResponse<GetUserOrganisationIdentifiersRequest>(
            new GetUserOrganisationIdentifiersResponse {
                NumericIdentifier = 42L,
                TextIdentifier = "ABCDE",
            }
        );

        autoMocker.MockResponse<GetOrganisationByIdRequest>(
            new GetOrganisationByIdResponse {
                Organisation = new Organisation {
                    Id = OrganisationId,
                    Name = "Test School",
                    Status = OrganisationStatus.Open,
                    LegacyId = 99L,
                    IsOnApar = "true",
                }
            }
        );

        return autoMocker;
    }

    [TestMethod]
    public async Task Throws_WhenUserHasNoAccess()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetUserServiceAccessRequest>(
            new GetUserServiceAccessResponse { Access = null }
        );

        var useCase = autoMocker.CreateInstance<GetUserAccessToServiceUseCase>();

        await Assert.ThrowsExactlyAsync<UserServiceAccessNotFoundException>(()
            => useCase.InvokeAsync(ValidRequest));
    }

    [TestMethod]
    public async Task Throws_WhenOrganisationNotFound()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetUserServiceAccessRequest>(
            new GetUserServiceAccessResponse {
                Access = new UserServiceAccess {
                    UserId = UserId,
                    ServiceId = ServiceId,
                    OrganisationId = OrganisationId,
                    Roles = [],
                    Identifiers = [],
                }
            }
        );

        autoMocker.MockResponse<GetUserOrganisationIdentifiersRequest>(
            new GetUserOrganisationIdentifiersResponse()
        );

        autoMocker.MockThrows<GetOrganisationByIdRequest>(
            OrganisationNotFoundException.FromOrganisationId(OrganisationId)
        );

        var useCase = autoMocker.CreateInstance<GetUserAccessToServiceUseCase>();

        await Assert.ThrowsExactlyAsync<OrganisationNotFoundException>(()
            => useCase.InvokeAsync(ValidRequest));
    }

    [TestMethod]
    public async Task ReturnsExpectedIds()
    {
        var autoMocker = SetupWithAccess();
        var useCase = autoMocker.CreateInstance<GetUserAccessToServiceUseCase>();

        var response = await useCase.InvokeAsync(ValidRequest);

        Assert.AreEqual(UserId, response.UserId);
        Assert.AreEqual(ServiceId, response.ServiceId);
        Assert.AreEqual(OrganisationId, response.OrganisationId);
    }

    [TestMethod]
    public async Task ReturnsUserLegacyIdentifiers()
    {
        var autoMocker = SetupWithAccess();
        var useCase = autoMocker.CreateInstance<GetUserAccessToServiceUseCase>();

        var response = await useCase.InvokeAsync(ValidRequest);

        Assert.AreEqual(42L, response.UserLegacyNumericId);
        Assert.AreEqual("ABCDE", response.UserLegacyTextId);
    }

    [TestMethod]
    public async Task ReturnsOrganisationLegacyIdAndApar()
    {
        var autoMocker = SetupWithAccess();
        var useCase = autoMocker.CreateInstance<GetUserAccessToServiceUseCase>();

        var response = await useCase.InvokeAsync(ValidRequest);

        Assert.AreEqual(99L, response.OrganisationLegacyId);
        Assert.AreEqual("true", response.OrganisationIsOnApar);
    }

    [TestMethod]
    public async Task ReturnsRolesFromAccessResponse()
    {
        var autoMocker = SetupWithAccess();
        var useCase = autoMocker.CreateInstance<GetUserAccessToServiceUseCase>();

        var response = await useCase.InvokeAsync(ValidRequest);

        var roles = response.Roles.ToArray();
        Assert.HasCount(1, roles);
        Assert.AreEqual("role-a", roles[0].Code);
    }

    [TestMethod]
    public async Task ReturnsIdentifiersFromAccessResponse()
    {
        var autoMocker = SetupWithAccess();
        var useCase = autoMocker.CreateInstance<GetUserAccessToServiceUseCase>();

        var response = await useCase.InvokeAsync(ValidRequest);

        var identifiers = response.Identifiers.ToArray();
        Assert.HasCount(1, identifiers);
        Assert.AreEqual("externalId", identifiers[0].Key);
        Assert.AreEqual("EXT-001", identifiers[0].Value);
    }

    [TestMethod]
    public async Task DispatchesAccessRequestWithCorrectIds()
    {
        var autoMocker = SetupWithAccess();
        GetUserServiceAccessRequest? capturedRequest = null;
        autoMocker.CaptureRequest<GetUserServiceAccessRequest>(
            r => capturedRequest = r,
            new GetUserServiceAccessResponse {
                Access = new UserServiceAccess {
                    UserId = UserId,
                    ServiceId = ServiceId,
                    OrganisationId = OrganisationId,
                    Roles = [],
                    Identifiers = [],
                }
            }
        );

        var useCase = autoMocker.CreateInstance<GetUserAccessToServiceUseCase>();
        await useCase.InvokeAsync(ValidRequest);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(UserId, capturedRequest.UserId);
        Assert.AreEqual(ServiceId, capturedRequest.ServiceId);
        Assert.AreEqual(OrganisationId, capturedRequest.OrganisationId);
    }
}
