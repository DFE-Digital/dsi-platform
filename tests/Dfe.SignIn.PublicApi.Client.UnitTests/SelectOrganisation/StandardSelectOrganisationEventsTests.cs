using Dfe.SignIn.Core.Public.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.Abstractions;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Dfe.SignIn.PublicApi.Contracts.Organisations;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class StandardSelectOrganisationEventsTests
{
    private static StandardSelectOrganisationUserFlowOptions SetupMockOptions(AutoMocker autoMocker)
    {
        var options = new StandardSelectOrganisationUserFlowOptions();

        autoMocker.GetMock<IOptions<StandardSelectOrganisationUserFlowOptions>>()
            .Setup(x => x.Value)
            .Returns(options);

        return options;
    }

    private static Mock<IHttpContext> SetupMockHttpContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<IHttpContext>();

        var mockResponse = autoMocker.GetMock<IHttpResponse>();
        mockContext.Setup(x => x.Response)
            .Returns(mockResponse.Object);

        return mockContext;
    }

    #region OnStartSelection(IHttpContext, Uri)

    [TestMethod]
    public async Task OnStartSelection_RedirectsToSelectionUri()
    {
        var autoMocker = new AutoMocker();
        var mockContext = SetupMockHttpContext(autoMocker);
        var events = autoMocker.CreateInstance<StandardSelectOrganisationEvents>();

        string expectedSelectionUri = "https://test.localhost/callback";

        await events.OnStartSelection(mockContext.Object, new Uri(expectedSelectionUri));

        autoMocker.Verify<IHttpResponse>(x =>
            x.Redirect(
                It.Is<string>(selectionUri => selectionUri == expectedSelectionUri)
            ),
            Times.Once
        );
    }

    #endregion

    #region OnCancelSelection(IHttpContext)

    [TestMethod]
    public async Task OnCancelSelection_RedirectsToSignOutPath_WhenActiveOrganisationStateIsNotPresent()
    {
        var autoMocker = new AutoMocker();
        var options = SetupMockOptions(autoMocker);
        var mockContext = SetupMockHttpContext(autoMocker);
        var events = autoMocker.CreateInstance<StandardSelectOrganisationEvents>();

        autoMocker.GetMock<IActiveOrganisationProvider>()
            .Setup(x => x.GetActiveOrganisationStateAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object)
            ))
            .ReturnsAsync((ActiveOrganisationState?)null);

        await events.OnCancelSelection(mockContext.Object);

        autoMocker.Verify<IHttpResponse>(x =>
            x.Redirect(
                It.Is<string>(selectionUri => selectionUri == options.SignOutPath)
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task OnCancelSelection_RedirectsToCompletedPath_WhenActiveOrganisationStateIsPresent()
    {
        var autoMocker = new AutoMocker();
        var options = SetupMockOptions(autoMocker);
        var mockContext = SetupMockHttpContext(autoMocker);
        var events = autoMocker.CreateInstance<StandardSelectOrganisationEvents>();

        autoMocker.GetMock<IActiveOrganisationProvider>()
            .Setup(x => x.GetActiveOrganisationStateAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object)
            ))
            .ReturnsAsync(new ActiveOrganisationState());

        await events.OnCancelSelection(mockContext.Object);

        autoMocker.Verify<IHttpResponse>(x =>
            x.Redirect(
                It.Is<string>(selectionUri => selectionUri == options.CompletedPath)
            ),
            Times.Once
        );
    }

    #endregion

    #region OnConfirmSelection(IHttpContext, OrganisationDetails?)

    private static readonly OrganisationDetails FakeOrganisation = new() {
        Id = new Guid("1bce763f-cb38-49a8-813c-a786a753f0eb"),
        Name = "Example Organisation 1",
    };

    private static IEnumerable<object[]> OnConfirmSelection_RedirectsToCompletedPath_Scenarios
        => [[null!], [FakeOrganisation]];

    [DynamicData(nameof(OnConfirmSelection_RedirectsToCompletedPath_Scenarios))]
    [TestMethod]
    public async Task OnConfirmSelection_SetsActiveOrganisation(OrganisationDetails? expectedOrganisation)
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var mockContext = SetupMockHttpContext(autoMocker);
        var events = autoMocker.CreateInstance<StandardSelectOrganisationEvents>();

        await events.OnConfirmSelection(mockContext.Object, expectedOrganisation);

        autoMocker.Verify<IActiveOrganisationProvider>(x =>
            x.SetActiveOrganisationAsync(
                It.Is<IHttpContext>(context => context == mockContext.Object),
                It.Is<OrganisationDetails?>(organisation => organisation == expectedOrganisation)
            ),
            Times.Once
        );
    }

    [DynamicData(nameof(OnConfirmSelection_RedirectsToCompletedPath_Scenarios))]
    [TestMethod]
    public async Task OnConfirmSelection_RedirectsToCompletedPath(OrganisationDetails? expectedOrganisation)
    {
        var autoMocker = new AutoMocker();
        var options = SetupMockOptions(autoMocker);
        var mockContext = SetupMockHttpContext(autoMocker);
        var events = autoMocker.CreateInstance<StandardSelectOrganisationEvents>();

        await events.OnConfirmSelection(mockContext.Object, expectedOrganisation);

        autoMocker.Verify<IHttpResponse>(x =>
            x.Redirect(
                It.Is<string>(selectionUri => selectionUri == options.CompletedPath)
            ),
            Times.Once
        );
    }

    #endregion

    #region OnError(IHttpContext, string)

    [TestMethod]
    public async Task OnError_Throws()
    {
        var autoMocker = new AutoMocker();
        SetupMockOptions(autoMocker);
        var mockContext = SetupMockHttpContext(autoMocker);
        var events = autoMocker.CreateInstance<StandardSelectOrganisationEvents>();

        var exception = await Assert.ThrowsExactlyAsync<SelectOrganisationCallbackErrorException>(
            () => events.OnError(mockContext.Object, SelectOrganisationErrorCode.InvalidSelection)
        );
        Assert.AreEqual(SelectOrganisationErrorCode.InvalidSelection, exception.ErrorCode);
    }

    #endregion

    #region OnSignOut(IHttpContext)

    [TestMethod]
    public async Task OnSignOut_RedirectsToSignOutPath()
    {
        var autoMocker = new AutoMocker();
        var options = SetupMockOptions(autoMocker);
        var mockContext = SetupMockHttpContext(autoMocker);
        var events = autoMocker.CreateInstance<StandardSelectOrganisationEvents>();

        await events.OnSignOut(mockContext.Object);

        autoMocker.Verify<IHttpResponse>(x =>
            x.Redirect(
                It.Is<string>(selectionUri => selectionUri == options.SignOutPath)
            ),
            Times.Once
        );
    }

    #endregion
}
