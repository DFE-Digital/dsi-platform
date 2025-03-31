using AutoMapper;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.Applications;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.PublicApi.Endpoints.SelectOrganisation;
using Dfe.SignIn.PublicApi.ScopedSession;
using Moq;
using Moq.AutoMock;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.UnitTests.Endpoints.SelectOrganisation;

[TestClass]
public sealed class PostSelectOrganisationSessionTests
{
    private static readonly CreateSelectOrganisationSession_PublicApiRequest FakePublicApiMinimalRequest = new() {
        CallbackUrl = new Uri("https://example.localhost/callback"),
        UserId = new Guid("6c843439-4633-4369-af49-f8b04b2529bc"),
    };
    private static readonly CreateSelectOrganisationSession_PublicApiRequest FakePublicApiRequest = new() {
        CallbackUrl = new Uri("https://example.localhost/callback"),
        UserId = new Guid("6c843439-4633-4369-af49-f8b04b2529bc"),
        DetailLevel = OrganisationDetailLevel.Basic,
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

    private static readonly ApplicationModel FakeApplicationModel = new() {
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

        autoMocker.Use(new MapperConfiguration(cfg =>
            cfg.AddProfile<SelectOrganisationMappingProfile>()
        ).CreateMapper());

        return autoMocker;
    }

    private static IEnumerable<object[]> SelectOrganisationEndpoints_InvokesExpectedInteractionRequest_Parameters => [
        [FakePublicApiMinimalRequest],
        [FakePublicApiRequest],
    ];

    [DataTestMethod]
    [DynamicData(nameof(SelectOrganisationEndpoints_InvokesExpectedInteractionRequest_Parameters), DynamicDataSourceType.Property)]
    public async Task SelectOrganisationEndpoints_InvokesExpectedInteractionRequest(
        CreateSelectOrganisationSession_PublicApiRequest apiRequest)
    {
        var autoMocker = CreateAutoMocker();

        autoMocker.GetMock<IScopedSessionReader>()
            .Setup(x => x.Application)
            .Returns(FakeApplicationModel);

        await SelectOrganisationEndpoints.PostSelectOrganisationSession(
            apiRequest,
            autoMocker.Get<IScopedSessionReader>(),
            autoMocker.Get<IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>>(),
            autoMocker.Get<IMapper>()
        );

        autoMocker.Verify<IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>>(x =>
            x.InvokeAsync(
                It.Is<CreateSelectOrganisationSessionRequest>(request =>
                    request.ClientId == "test-client-id" &&
                    request.CallbackUrl == apiRequest.CallbackUrl &&
                    request.UserId == apiRequest.UserId &&
                    request.DetailLevel == apiRequest.DetailLevel &&
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
            .Returns(FakeApplicationModel);

        var fakeResponse = new CreateSelectOrganisationSessionResponse {
            HasOptions = true,
            Url = new Uri("https://select-organisation.localhost"),
        };
        autoMocker.GetMock<IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>>()
            .Setup(x => x.InvokeAsync(
                It.IsAny<CreateSelectOrganisationSessionRequest>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(fakeResponse);

        var response = await SelectOrganisationEndpoints.PostSelectOrganisationSession(
            FakePublicApiRequest,
            autoMocker.Get<IScopedSessionReader>(),
            autoMocker.Get<IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>>(),
            autoMocker.Get<IMapper>()
        );

        Assert.AreEqual(fakeResponse.Url, response.Url);
    }
}
