using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.UseCases.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.Users;

[TestClass]
public sealed class GetUserOrganisationsUseCaseTests
{
    private static readonly Guid UserId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001");

    private static readonly GetUserOrganisationsRequest ValidRequest = new() {
        UserId = UserId,
    };

    private static readonly Organisation OpenOrg = new() {
        Id = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000010"),
        Name = "Open Org",
        Status = OrganisationStatus.Open,
    };

    private static readonly Organisation HiddenOrg = new() {
        Id = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000011"),
        Name = "Hidden Org",
        Status = OrganisationStatus.Hidden,
    };

    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetUserOrganisationsRequest,
            GetUserOrganisationsUseCase
        >();
    }

    [TestMethod]
    public async Task Throws_WhenNoOrganisationsReturned()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetOrganisationsAssociatedWithUserRequest>(
            new GetOrganisationsAssociatedWithUserResponse { Organisations = [] }
        );

        var useCase = autoMocker.CreateInstance<GetUserOrganisationsUseCase>();

        await Assert.ThrowsExactlyAsync<UserNotFoundException>(()
            => useCase.InvokeAsync(ValidRequest));
    }

    [TestMethod]
    public async Task Throws_WhenAllOrganisationsAreHidden()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetOrganisationsAssociatedWithUserRequest>(
            new GetOrganisationsAssociatedWithUserResponse {
                Organisations = [HiddenOrg],
            }
        );

        var useCase = autoMocker.CreateInstance<GetUserOrganisationsUseCase>();

        await Assert.ThrowsExactlyAsync<UserNotFoundException>(()
            => useCase.InvokeAsync(ValidRequest));
    }

    [TestMethod]
    public async Task FiltersOutHiddenOrganisations()
    {
        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetOrganisationsAssociatedWithUserRequest>(
            new GetOrganisationsAssociatedWithUserResponse {
                Organisations = [OpenOrg, HiddenOrg],
            }
        );

        var useCase = autoMocker.CreateInstance<GetUserOrganisationsUseCase>();
        var response = await useCase.InvokeAsync(ValidRequest);

        var orgs = response.Organisations.ToArray();
        Assert.HasCount(1, orgs);
        Assert.AreEqual(OpenOrg.Id, orgs[0].Id);
    }

    [TestMethod]
    public async Task ReturnsAllVisibleOrganisations()
    {
        var org2 = new Organisation {
            Id = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000012"),
            Name = "Open Org 2",
            Status = OrganisationStatus.Open,
        };

        var autoMocker = new AutoMocker();

        autoMocker.MockResponse<GetOrganisationsAssociatedWithUserRequest>(
            new GetOrganisationsAssociatedWithUserResponse {
                Organisations = [OpenOrg, org2],
            }
        );

        var useCase = autoMocker.CreateInstance<GetUserOrganisationsUseCase>();
        var response = await useCase.InvokeAsync(ValidRequest);

        Assert.HasCount(2, response.Organisations.ToArray());
    }

    [TestMethod]
    public async Task DispatchesRequestWithCorrectUserId()
    {
        var autoMocker = new AutoMocker();
        GetOrganisationsAssociatedWithUserRequest? capturedRequest = null;

        autoMocker.CaptureRequest<GetOrganisationsAssociatedWithUserRequest>(
            r => capturedRequest = r,
            new GetOrganisationsAssociatedWithUserResponse {
                Organisations = [OpenOrg],
            }
        );

        var useCase = autoMocker.CreateInstance<GetUserOrganisationsUseCase>();
        await useCase.InvokeAsync(ValidRequest);

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(UserId, capturedRequest.UserId);
    }
}
