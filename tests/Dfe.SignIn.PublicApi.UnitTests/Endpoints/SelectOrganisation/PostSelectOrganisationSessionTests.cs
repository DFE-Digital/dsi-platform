using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Public.SelectOrganisation;
using Dfe.SignIn.PublicApi.Contracts.SelectOrganisation;
using Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;
using Dfe.SignIn.PublicApi.ScopedSession;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.SelectOrganisation;

[TestClass]
public sealed class PostSelectOrganisationSessionTests
{
    private static readonly CreateSelectOrganisationSessionApiRequest FakePublicApiMinimalRequest = new() {
        CallbackUrl = new Uri("https://example.localhost/callback"),
        UserId = new Guid("6c843439-4633-4369-af49-f8b04b2529bc"),
    };
    private static readonly CreateSelectOrganisationSessionApiRequest FakePublicApiRequest = new() {
        CallbackUrl = new Uri("https://example.localhost/callback"),
        UserId = new Guid("6c843439-4633-4369-af49-f8b04b2529bc"),
        Filter = new OrganisationFilter {
            Association = OrganisationFilterAssociation.AssignedToUser,
            OrganisationIds = [],
            Type = OrganisationFilterType.Associated,
        },
        Prompt = new SelectOrganisationPrompt {
            Heading = "Which organisation?",
            Hint = "Select one option.",
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

        autoMocker.Use(FakePublicApiRequest);

        return autoMocker;
    }

    private static IEnumerable<object[]> SelectOrganisationEndpoints_InvokesExpectedInteractionRequest_Parameters => [
        [FakePublicApiMinimalRequest],
        [FakePublicApiRequest],
    ];

    private static readonly CreateSelectOrganisationSessionResponse FakeResponse = new() {
        RequestId = new Guid("fba90ce7-b5d0-4f94-ae00-63a8d21bde93"),
        HasOptions = true,
        Url = new Uri("https://select-organisation.localhost"),
    };

    [DataTestMethod]
    [DynamicData(nameof(SelectOrganisationEndpoints_InvokesExpectedInteractionRequest_Parameters), DynamicDataSourceType.Property)]
    public async Task SelectOrganisationEndpoints_InvokesExpectedInteractionRequest(
        CreateSelectOrganisationSessionApiRequest apiRequest)
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.GetMock<IScopedSessionReader>()
            .Setup(x => x.Application)
            .Returns(FakeApplication);

        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<CreateSelectOrganisationSessionRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(InteractionTask.FromResult(FakeResponse));

        await SelectOrganisationEndpoints.PostSelectOrganisationSession(
            apiRequest,
            autoMocker.Get<IScopedSessionReader>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        autoMocker.Verify<IInteractionDispatcher, InteractionTask>(x =>
            x.DispatchAsync(
                It.Is<CreateSelectOrganisationSessionRequest>(request =>
                    request.ClientId == "test-client-id" &&
                    request.CallbackUrl == apiRequest.CallbackUrl &&
                    request.UserId == apiRequest.UserId &&
                    request.Filter == apiRequest.Filter &&
                    request.Prompt == apiRequest.Prompt
                ),
                It.IsAny<CancellationToken>()
            )
        );
    }

    [TestMethod]
    public async Task SelectOrganisationEndpoints_ReturnsExpectedResponse()
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.GetMock<IScopedSessionReader>()
            .Setup(x => x.Application)
            .Returns(FakeApplication);

        autoMocker.GetMock<IInteractionDispatcher>()
            .Setup(x => x.DispatchAsync(
                It.IsAny<CreateSelectOrganisationSessionRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .Returns(InteractionTask.FromResult(FakeResponse));

        var response = await SelectOrganisationEndpoints.PostSelectOrganisationSession(
            FakePublicApiRequest,
            autoMocker.Get<IScopedSessionReader>(),
            autoMocker.Get<IInteractionDispatcher>()
        );

        Assert.AreEqual(FakeResponse.RequestId, response.RequestId);
        Assert.AreEqual(FakeResponse.HasOptions, response.HasOptions);
        Assert.AreEqual(FakeResponse.Url, response.Url);
    }
}
