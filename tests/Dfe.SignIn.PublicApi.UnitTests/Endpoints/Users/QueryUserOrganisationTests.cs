using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.Public.SelectOrganisation;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Contracts.Users;
using Dfe.SignIn.PublicApi.Endpoints.Users;
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

    private const string FakeApplicationClientId = "test-client-id";

    private static AutoMocker CreateAutoMocker()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IClientSession>()
            .Setup(x => x.ClientId)
            .Returns(FakeApplicationClientId);

        return autoMocker;
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

        FilterOrganisationsForUserRequest? capturedRequest = null;
        autoMocker.CaptureRequest<FilterOrganisationsForUserRequest>(
            request => capturedRequest = request,
            new FilterOrganisationsForUserResponse {
                FilteredOrganisations = [],
            }
        );

        await UserEndpoints.PostQueryUserOrganisation(
            FakeUserId,
            FakeOrganisation1.Id,
            apiRequest,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual("test-client-id", capturedRequest.ClientId);
        Assert.AreEqual(FakeUserId, capturedRequest.UserId);
        Assert.AreEqual(apiRequest.Filter, capturedRequest.Filter);
    }

    [TestMethod]
    public async Task PostQueryUserOrganisation_ReturnsExpectedResponse_WhenOrganisationWasNotMatched()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<FilterOrganisationsForUserRequest>(
            new FilterOrganisationsForUserResponse {
                FilteredOrganisations = [],
            }
        );

        var response = await UserEndpoints.PostQueryUserOrganisation(
            FakeUserId,
            FakeOrganisation1.Id,
            FakeMinimalRequest,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.AreEqual(FakeUserId, response.UserId);
        Assert.IsNull(response.Organisation);
    }

    [TestMethod]
    public async Task PostQueryUserOrganisation_ReturnsExpectedResponse_WhenOrganisationWasMatched()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.MockResponse<FilterOrganisationsForUserRequest>(
            new FilterOrganisationsForUserResponse {
                FilteredOrganisations = [
                    FakeOrganisation1,
                    FakeOrganisation2,
                ],
            }
        );

        var response = await UserEndpoints.PostQueryUserOrganisation(
            FakeUserId,
            FakeOrganisation1.Id,
            FakeMinimalRequest,
            autoMocker.Get<IClientSession>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.AreEqual(FakeUserId, response.UserId);

        Assert.IsNotNull(response.Organisation);
        Assert.AreEqual(FakeOrganisation1.Id, response.Organisation.Id);
        Assert.AreEqual(FakeOrganisation1.Name, response.Organisation.Name);
    }
}
