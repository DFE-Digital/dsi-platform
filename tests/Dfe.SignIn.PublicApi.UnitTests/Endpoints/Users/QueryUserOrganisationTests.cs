using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.Public.SelectOrganisation;
using Dfe.SignIn.PublicApi.Contracts.Users;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Dfe.SignIn.PublicApi.ScopedSession;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.Users;

[TestClass]
public sealed class QueryUserOrganisationTests
{
    private static readonly Guid FakeUserId = new("6c843439-4633-4369-af49-f8b04b2529bc");

    private static readonly Organisation FakeOrganisation1 = new() {
        Id = new Guid("ad55df19-3d10-4089-b15f-bba6f546009f"),
        Name = "Example Organisation 1",
        Status = OrganisationStatus.Open,
    };

    private static readonly Organisation FakeOrganisation2 = new() {
        Id = new Guid("e27df6e3-84ab-40ab-84b2-4a85a7fa11cd"),
        Name = "Example Organisation 2",
        Status = OrganisationStatus.Open,
    };

    private static readonly QueryUserOrganisationApiRequestBody FakeMinimalRequest = new();

    private static readonly QueryUserOrganisationApiRequestBody FakeDetailedRequest = new() {
        Filter = new OrganisationFilter {
            Association = OrganisationFilterAssociation.AssignedToUser,
            OrganisationIds = [],
            Type = OrganisationFilterType.Associated,
        },
    };

    private static readonly Application FakeApplication = new() {
        Id = Guid.Empty,
        ClientId = "test-client-id",
        Name = "Test application",
        IsExternalService = false,
        IsHiddenService = false,
        IsIdOnlyService = false,
        ServiceHomeUrl = new Uri("https://service.localhost"),
    };

    private static AutoMocker CreateAutoMocker()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IScopedSessionReader>()
            .Setup(x => x.Application)
            .Returns(FakeApplication);

        return autoMocker;
    }

    private static void SetupFakeFilteredOrganisationsResponse(
        AutoMocker autoMocker,
        params Organisation[] filteredOrganisations)
    {
        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<FilterOrganisationsForUserRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(InteractionTask.FromResult(new FilterOrganisationsForUserResponse {
                FilteredOrganisations = filteredOrganisations,
            }));
    }

    private static IEnumerable<object[]> PostQueryUserOrganisation_InvokesExpectedInteractionRequest_Parameters => [
        [FakeMinimalRequest],
        [FakeDetailedRequest],
    ];

    [TestMethod]
    [DynamicData(nameof(PostQueryUserOrganisation_InvokesExpectedInteractionRequest_Parameters), DynamicDataSourceType.Property)]
    public async Task PostQueryUserOrganisation_InvokesExpectedInteractionRequest(
        QueryUserOrganisationApiRequestBody apiRequest)
    {
        var autoMocker = CreateAutoMocker();
        SetupFakeFilteredOrganisationsResponse(autoMocker);

        await UserEndpoints.PostQueryUserOrganisation(
            FakeUserId,
            FakeOrganisation1.Id,
            apiRequest,
            autoMocker.Get<IScopedSessionReader>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        autoMocker.Verify<IInteractionDispatcher, InteractionTask>(x =>
            x.DispatchAsync(
                It.Is<FilterOrganisationsForUserRequest>(request =>
                    request.ClientId == "test-client-id" &&
                    request.UserId == FakeUserId &&
                    request.Filter == apiRequest.Filter
                ),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [TestMethod]
    public async Task PostQueryUserOrganisation_ReturnsExpectedResponse_WhenOrganisationWasNotMatched()
    {
        var autoMocker = CreateAutoMocker();
        SetupFakeFilteredOrganisationsResponse(autoMocker);

        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<FilterOrganisationsForUserRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(InteractionTask.FromResult(new FilterOrganisationsForUserResponse {
                FilteredOrganisations = [],
            }));

        var response = await UserEndpoints.PostQueryUserOrganisation(
            FakeUserId,
            FakeOrganisation1.Id,
            FakeMinimalRequest,
            autoMocker.Get<IScopedSessionReader>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.AreEqual(FakeUserId, response.UserId);
        Assert.IsNull(response.Organisation);
    }

    [TestMethod]
    public async Task PostQueryUserOrganisation_ReturnsExpectedResponse_WhenOrganisationWasMatched()
    {
        var autoMocker = CreateAutoMocker();
        SetupFakeFilteredOrganisationsResponse(autoMocker);

        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<FilterOrganisationsForUserRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(InteractionTask.FromResult(new FilterOrganisationsForUserResponse {
                FilteredOrganisations = [
                    FakeOrganisation1,
                    FakeOrganisation2,
                ],
            }));

        var response = await UserEndpoints.PostQueryUserOrganisation(
            FakeUserId,
            FakeOrganisation1.Id,
            FakeMinimalRequest,
            autoMocker.Get<IScopedSessionReader>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.AreEqual(FakeUserId, response.UserId);

        Assert.IsNotNull(response.Organisation);
        Assert.AreEqual(FakeOrganisation1.Id, response.Organisation.Id);
        Assert.AreEqual(FakeOrganisation1.Name, response.Organisation.Name);
    }
}
