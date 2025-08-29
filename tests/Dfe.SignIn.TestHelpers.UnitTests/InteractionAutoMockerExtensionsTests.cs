using Dfe.SignIn.Core.Framework;
using Moq.AutoMock;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class InteractionAutoMockerExtensionsTests
{
    private sealed record ExampleRequest
    {
        public int Value { get; init; }
    }

    private sealed record ExampleResponse
    {
    }

    private sealed record ExampleOtherRequest
    {
    }

    #region CaptureRequest<TRequest>(AutoMocker, Action<TRequest>)

    [TestMethod]
    public async Task CaptureRequest_CapturesRequestModel()
    {
        var autoMocker = new AutoMocker();

        ExampleRequest? capturedRequest = null;
        autoMocker.CaptureRequest<ExampleRequest>(r => capturedRequest = r);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        await mockInteraction.Object.DispatchAsync(new ExampleRequest {
            Value = 42,
        });

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(42, capturedRequest.Value);
    }

    #endregion

    #region MockResponse<TRequest>(AutoMocker, object)

    [TestMethod]
    public async Task MockResponse_AnyRequestOfTypeReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();

        var fakeResponse = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponse<ExampleRequest>(autoMocker, fakeResponse);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        var actualResponse = await mockInteraction.Object.DispatchAsync(new ExampleRequest());

        Assert.AreSame(fakeResponse, actualResponse);
    }

    #endregion

    #region MockResponse<TRequest>(AutoMocker, TRequest, object)

    [TestMethod]
    public async Task MockResponse_SpecificRequestReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();

        var fakeRequestA = new ExampleRequest();
        var fakeRequestB = new ExampleRequest();

        var fakeResponseA = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponse(autoMocker, fakeRequestA, fakeResponseA);
        var fakeResponseB = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponse(autoMocker, fakeRequestB, fakeResponseB);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        var actualResponseA = await mockInteraction.Object.DispatchAsync(fakeRequestA);
        var actualResponseB = await mockInteraction.Object.DispatchAsync(fakeRequestB);

        Assert.AreSame(fakeResponseA, actualResponseA);
        Assert.AreSame(fakeResponseB, actualResponseB);
    }

    #endregion

    #region MockResponse<TRequest, TResponse>(AutoMocker)

    [TestMethod]
    public async Task MockResponse_AnyRequestOfTypeReturnsEmptyResponse()
    {
        var autoMocker = new AutoMocker();

        InteractionAutoMockerExtensions.MockResponse<ExampleRequest, ExampleResponse>(autoMocker);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        var actualResponse = await mockInteraction.Object.DispatchAsync(new ExampleRequest());

        Assert.IsNotNull(actualResponse);
        Assert.IsInstanceOfType<ExampleResponse>(actualResponse);
    }

    #endregion

    #region MockThrows<TRequest>(AutoMocker, Exception)

    [TestMethod]
    public async Task MockThrows_AnyRequestOfTypeThrowsExpectedException()
    {
        var autoMocker = new AutoMocker();

        var expectedException = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrows<ExampleRequest>(autoMocker, expectedException);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();

        async Task Act()
        {
            await mockInteraction.Object.DispatchAsync(new ExampleRequest());
        }

        var actualException = await Assert.ThrowsAsync<InvalidRequestException>(Act);

        Assert.AreSame(expectedException, actualException);
    }

    [TestMethod]
    public async Task MockThrows_OtherRequestsDoNotThrow()
    {
        var autoMocker = new AutoMocker();

        var expectedException = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrows<ExampleRequest>(autoMocker, expectedException);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();

        try {
            await mockInteraction.Object.DispatchAsync(new ExampleOtherRequest());
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion

    #region MockThrows<TRequest>(AutoMocker, TRequest, Exception)

    [TestMethod]
    public async Task MockThrows_SpecificRequestThrowsExpectedException()
    {
        var autoMocker = new AutoMocker();

        var fakeRequestA = new ExampleRequest();
        var fakeRequestB = new ExampleRequest();

        var expectedException = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrows(autoMocker, fakeRequestA, expectedException);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();

        // The specific request throws.
        async Task Act()
        {
            await mockInteraction.Object.DispatchAsync(fakeRequestA);
        }

        var actualException = await Assert.ThrowsAsync<InvalidRequestException>(Act);

        // Any other requests do not throw.
        await mockInteraction.Object.DispatchAsync(fakeRequestB);

        Assert.AreSame(expectedException, actualException);
    }

    #endregion
}
