using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackProcessorExtensionsTests
{
    private static Mock<HttpRequest> CreateMockRequestWithFormData()
    {
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(mock => mock.Form)
            .Returns(new FormCollection(new() {
                { "payloadType", PayloadTypeConstants.Id },
                { "payload", "{data}" },
                { "sig", "{sig}" },
                { "kid", "6bb65413-12db-41b9-a606-82103e6d5c0c" },
            }));
        return mockRequest;
    }

    private static readonly SelectOrganisationCallbackViewModel FakeSelectOrganisationCallbackViewModel = new() {
        PayloadType = PayloadTypeConstants.Id,
        Payload = "{data}",
        Sig = "{sig}",
        Kid = "6bb65413-12db-41b9-a606-82103e6d5c0c",
    };

    #region ViewModelFromRequest(HttpRequest)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ViewModelFromRequest_Throws_WhenRequestArgumentIsNull()
    {
        SelectOrganisationCallbackProcessorExtensions.ViewModelFromRequest(null!);
    }

    [TestMethod]
    public void ViewModelFromRequest_PopulatesViewModelFromRequest()
    {
        var mockRequest = CreateMockRequestWithFormData();

        var viewModel = SelectOrganisationCallbackProcessorExtensions.ViewModelFromRequest(mockRequest.Object);

        Assert.AreEqual(FakeSelectOrganisationCallbackViewModel, viewModel);
    }

    #endregion

    #region ResolveCallbackType(string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ResolveCallbackType_Throws_WhenPayloadTypeArgumentIsNull()
    {
        SelectOrganisationCallbackProcessorExtensions.ResolveCallbackType(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ResolveCallbackType_Throws_WhenPayloadTypeArgumentIsEmptyString()
    {
        SelectOrganisationCallbackProcessorExtensions.ResolveCallbackType("   ");
    }

    [DataRow(PayloadTypeConstants.Error, typeof(SelectOrganisationCallbackError))]
    [DataRow(PayloadTypeConstants.Id, typeof(SelectOrganisationCallbackId))]
    [DataRow(PayloadTypeConstants.Basic, typeof(SelectOrganisationCallbackBasic))]
    [DataRow(PayloadTypeConstants.Extended, typeof(SelectOrganisationCallbackExtended))]
    [DataRow(PayloadTypeConstants.Legacy, typeof(SelectOrganisationCallbackLegacy))]
    [DataTestMethod]
    public void ResolveCallbackType_MapsPayloadTypeToExpectedRuntimeType(string payloadType, Type expectedType)
    {
        var result = SelectOrganisationCallbackProcessorExtensions.ResolveCallbackType(payloadType);

        Assert.AreEqual(expectedType, result);
    }

    [TestMethod]
    public void ResolveCallbackType_Throws_WhenCalledWithUnexpectedPayloadType()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(
            () => SelectOrganisationCallbackProcessorExtensions.ResolveCallbackType("foo")
        );
        Assert.AreEqual("Unexpected callback type 'foo'.", exception.Message);
    }

    #endregion

    #region ProcessCallbackJsonAsync(ISelectOrganisationCallbackProcessor, HttpRequest)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ProcessCallbackJsonAsync_Throws_WhenProcessorArgumentIsNull()
    {
        var mockRequest = new Mock<HttpRequest>();

        SelectOrganisationCallbackProcessorExtensions.ProcessCallbackJsonAsync(null!, mockRequest.Object);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ProcessCallbackJsonAsync_Throws_WhenRequestArgumentIsNull()
    {
        var mockProcessor = new Mock<ISelectOrganisationCallbackProcessor>();

        SelectOrganisationCallbackProcessorExtensions.ProcessCallbackJsonAsync(mockProcessor.Object, null!);
    }

    [TestMethod]
    public async Task ProcessCallbackJsonAsync_InvokeWithViewModel()
    {
        var mockProcessor = new Mock<ISelectOrganisationCallbackProcessor>();
        var mockRequest = CreateMockRequestWithFormData();

        await mockProcessor.Object.ProcessCallbackJsonAsync(mockRequest.Object);

        mockProcessor.Verify(x =>
            x.ProcessCallbackJsonAsync(
                It.Is<SelectOrganisationCallbackViewModel>(viewModel => viewModel == FakeSelectOrganisationCallbackViewModel),
                It.Is<bool>(throwOnError => throwOnError)
            ),
            Times.Once
        );
    }

    #endregion

    #region ProcessCallbackAsync(ISelectOrganisationCallbackProcessor, HttpRequest)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackAsync_Throws_WhenProcessorArgumentIsNull()
    {
        var mockRequest = new Mock<HttpRequest>();

        await SelectOrganisationCallbackProcessorExtensions.ProcessCallbackAsync(null!, mockRequest.Object);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackAsync_Throws_WhenRequestArgumentIsNull()
    {
        var mockProcessor = new Mock<ISelectOrganisationCallbackProcessor>();

        await SelectOrganisationCallbackProcessorExtensions.ProcessCallbackAsync(mockProcessor.Object, null!);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_InvokeWithViewModel()
    {
        var expectedCallbackData = new SelectOrganisationCallbackId {
            Type = PayloadTypeConstants.Id,
            Id = new Guid("35553168-5f6b-44db-bc69-ee6227372cc6"),
        };

        var mockProcessor = new Mock<ISelectOrganisationCallbackProcessor>();
        mockProcessor
            .Setup(mock => mock.ProcessCallbackRawAsync(
                It.Is<SelectOrganisationCallbackViewModel>(viewModel => viewModel == FakeSelectOrganisationCallbackViewModel),
                It.Is<Type>(targetType => targetType == typeof(SelectOrganisationCallbackId)),
                It.Is<bool>(throwOnError => throwOnError)
            ))
            .ReturnsAsync(expectedCallbackData);

        var mockRequest = CreateMockRequestWithFormData();

        var result = await mockProcessor.Object.ProcessCallbackAsync(mockRequest.Object);

        Assert.AreEqual(expectedCallbackData, result);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_Throws_WhenNullIsReturned()
    {
        var mockProcessor = new Mock<ISelectOrganisationCallbackProcessor>();
        mockProcessor
            .Setup(mock => mock.ProcessCallbackRawAsync(
                It.Is<SelectOrganisationCallbackViewModel>(viewModel => viewModel == FakeSelectOrganisationCallbackViewModel),
                It.Is<Type>(targetType => targetType == typeof(SelectOrganisationCallbackId)),
                It.Is<bool>(throwOnError => throwOnError)
            ))
            .ReturnsAsync(null!);

        var mockRequest = CreateMockRequestWithFormData();

        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => mockProcessor.Object.ProcessCallbackAsync(mockRequest.Object)
        );
        Assert.AreEqual("Invalid callback data.", exception.Message);
    }

    #endregion

    #region ProcessCallbackAsync<TCallback>(ISelectOrganisationCallbackProcessor, HttpRequest)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackAsync_TCallback_Throws_WhenProcessorArgumentIsNull()
    {
        var mockRequest = new Mock<HttpRequest>();

        await SelectOrganisationCallbackProcessorExtensions.ProcessCallbackAsync<SelectOrganisationCallbackBasic>(
            null!, mockRequest.Object);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task ProcessCallbackAsync_TCallback_Throws_WhenRequestArgumentIsNull()
    {
        var mockProcessor = new Mock<ISelectOrganisationCallbackProcessor>();

        await SelectOrganisationCallbackProcessorExtensions.ProcessCallbackAsync<SelectOrganisationCallbackBasic>(
            mockProcessor.Object, null!);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_TCallback_InvokeWithViewModel()
    {
        var expectedCallbackData = new SelectOrganisationCallbackId {
            Type = PayloadTypeConstants.Id,
            Id = new Guid("35553168-5f6b-44db-bc69-ee6227372cc6"),
        };

        var mockProcessor = new Mock<ISelectOrganisationCallbackProcessor>();
        mockProcessor
            .Setup(mock => mock.ProcessCallbackRawAsync(
                It.Is<SelectOrganisationCallbackViewModel>(viewModel => viewModel == FakeSelectOrganisationCallbackViewModel),
                It.Is<Type>(targetType => targetType == typeof(SelectOrganisationCallbackId)),
                It.Is<bool>(throwOnError => throwOnError)
            ))
            .ReturnsAsync(expectedCallbackData);

        var mockRequest = CreateMockRequestWithFormData();

        var result = await mockProcessor.Object.ProcessCallbackAsync<SelectOrganisationCallbackId>(mockRequest.Object);

        Assert.AreEqual(expectedCallbackData, result);
    }

    [TestMethod]
    public async Task ProcessCallbackAsync_TCallback_Throws_WhenNullIsReturned()
    {
        var mockProcessor = new Mock<ISelectOrganisationCallbackProcessor>();
        mockProcessor
            .Setup(mock => mock.ProcessCallbackRawAsync(
                It.Is<SelectOrganisationCallbackViewModel>(viewModel => viewModel == FakeSelectOrganisationCallbackViewModel),
                It.Is<Type>(targetType => targetType == typeof(SelectOrganisationCallbackId)),
                It.Is<bool>(throwOnError => throwOnError)
            ))
            .ReturnsAsync(null!);

        var mockRequest = CreateMockRequestWithFormData();

        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => mockProcessor.Object.ProcessCallbackAsync<SelectOrganisationCallbackId>(mockRequest.Object)
        );
        Assert.AreEqual("Invalid callback data.", exception.Message);
    }

    #endregion
}
