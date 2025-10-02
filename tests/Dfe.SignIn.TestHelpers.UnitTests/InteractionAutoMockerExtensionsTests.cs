using Dfe.SignIn.Base.Framework;
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
        await mockInteraction.Object.DispatchAsync(
            new ExampleRequest { Value = 42 }
        );

        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(42, capturedRequest.Value);
    }

    [TestMethod]
    public async Task CaptureRequest_OverridesResponseWithNull()
    {
        var autoMocker = new AutoMocker();

        autoMocker.CaptureRequest<ExampleRequest>(_ => { });

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        var response = await mockInteraction.Object.DispatchAsync(
            new ExampleRequest { Value = 42 }
        );

        Assert.IsNull(response);
    }

    [TestMethod]
    public async Task CaptureRequest_UsesProvidedResponse()
    {
        var autoMocker = new AutoMocker();
        var fakeResponse = new ExampleResponse();

        autoMocker.CaptureRequest<ExampleRequest>(_ => { }, fakeResponse);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        var response = await mockInteraction.Object.DispatchAsync(
            new ExampleRequest { Value = 42 }
        );

        Assert.AreSame(fakeResponse, response);
    }

    #endregion

    #region MockResponseWhereContext<TRequest>(AutoMocker, TRequest, object)

    [TestMethod]
    public async Task MockResponseWhereContext_MatchedRequestReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();

        var fakeRequestA = new ExampleRequest { Value = 123 };
        var fakeRequestB = new ExampleRequest { Value = 456 };

        var fakeResponseA = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponseWhereContext<ExampleRequest>(
            autoMocker, ctx => ReferenceEquals(ctx.Request, fakeRequestA), fakeResponseA);

        var fakeResponseB = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponseWhereContext<ExampleRequest>(
            autoMocker, ctx => ReferenceEquals(ctx.Request, fakeRequestB), fakeResponseB);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        var actualResponseA = await mockInteraction.Object.DispatchAsync(fakeRequestA);
        var actualResponseB = await mockInteraction.Object.DispatchAsync(fakeRequestB);

        Assert.AreEqual(fakeResponseA, actualResponseA);
        Assert.AreEqual(fakeResponseB, actualResponseB);
    }

    #endregion

    #region MockResponseWhere<TRequest>(AutoMocker, TRequest, object)

    [TestMethod]
    public async Task MockResponseWhere_MatchedRequestReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();

        var fakeRequestA = new ExampleRequest { Value = 123 };
        var fakeRequestB = new ExampleRequest { Value = 456 };

        var fakeResponseA = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponseWhere<ExampleRequest>(
            autoMocker, req => req.Value == 123, fakeResponseA);

        var fakeResponseB = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponseWhere<ExampleRequest>(
            autoMocker, req => req.Value == 456, fakeResponseB);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        var actualResponseA = await mockInteraction.Object.DispatchAsync(fakeRequestA);
        var actualResponseB = await mockInteraction.Object.DispatchAsync(fakeRequestB);

        Assert.AreEqual(fakeResponseA, actualResponseA);
        Assert.AreEqual(fakeResponseB, actualResponseB);
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

        Assert.AreEqual(fakeResponseA, actualResponseA);
        Assert.AreEqual(fakeResponseB, actualResponseB);
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

    #region MockResponseExactly<TRequest>(AutoMocker, TRequest, object)

    [TestMethod]
    public async Task MockResponseExactly_SpecificRequestReturnsExpectedResponse()
    {
        var autoMocker = new AutoMocker();

        var fakeRequestA = new ExampleRequest();
        var fakeRequestB = new ExampleRequest();

        var fakeResponseA = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponseExactly(autoMocker, fakeRequestA, fakeResponseA);
        var fakeResponseB = new ExampleResponse();
        InteractionAutoMockerExtensions.MockResponseExactly(autoMocker, fakeRequestB, fakeResponseB);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();
        var actualResponseA = await mockInteraction.Object.DispatchAsync(fakeRequestA);
        var actualResponseB = await mockInteraction.Object.DispatchAsync(fakeRequestB);

        Assert.AreSame(fakeResponseA, actualResponseA);
        Assert.AreSame(fakeResponseB, actualResponseB);
    }

    #endregion

    #region MockThrowsWhereContext<TRequest>(AutoMocker, TRequest, object)

    [TestMethod]
    public async Task MockThrowsWhereContext_MatchedRequestThrowsExpectedException()
    {
        var autoMocker = new AutoMocker();

        var fakeRequestA = new ExampleRequest { Value = 123 };
        var fakeRequestB = new ExampleRequest { Value = 456 };

        var expectedExceptionA = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrowsWhereContext<ExampleRequest>(
            autoMocker, ctx => ReferenceEquals(ctx.Request, fakeRequestA), expectedExceptionA);

        var expectedExceptionB = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrowsWhereContext<ExampleRequest>(
            autoMocker, ctx => ReferenceEquals(ctx.Request, fakeRequestB), expectedExceptionB);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();

        // The targeted request throws.
        var actualExceptionA = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await mockInteraction.Object.DispatchAsync(fakeRequestA));

        // The targeted request throws.
        var actualExceptionB = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await mockInteraction.Object.DispatchAsync(fakeRequestB));

        // Other requests do not throw.
        try {
            await mockInteraction.Object.DispatchAsync(new ExampleRequest {
                Value = 456,
            });
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion

    #region MockThrowsWhere<TRequest>(AutoMocker, TRequest, object)

    [TestMethod]
    public async Task MockThrowsWhere_MatchedRequestThrowsExpectedException()
    {
        var autoMocker = new AutoMocker();

        var fakeRequestA = new ExampleRequest { Value = 123 };
        var fakeRequestB = new ExampleRequest { Value = 456 };

        var expectedExceptionA = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrowsWhere<ExampleRequest>(
            autoMocker, req => req.Value == 123, expectedExceptionA);

        var expectedExceptionB = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrowsWhere<ExampleRequest>(
            autoMocker, req => req.Value == 456, expectedExceptionB);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();

        // The targeted request throws.
        var actualExceptionA = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await mockInteraction.Object.DispatchAsync(fakeRequestA));
        Assert.AreSame(expectedExceptionA, actualExceptionA);

        // The targeted request throws.
        var actualExceptionB = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await mockInteraction.Object.DispatchAsync(fakeRequestB));
        Assert.AreSame(expectedExceptionB, actualExceptionB);

        // Other requests do not throw.
        try {
            await mockInteraction.Object.DispatchAsync(new ExampleRequest {
                Value = 789,
            });
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
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

        // The targeted request throws.
        var actualException = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await mockInteraction.Object.DispatchAsync(new ExampleRequest()));
        Assert.AreSame(expectedException, actualException);

        // Other requests do not throw.
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
    public async Task MockThrows_RequestThrowsExpectedException()
    {
        var autoMocker = new AutoMocker();

        var fakeRequestA = new ExampleRequest { Value = 123 };
        var fakeRequestB = new ExampleRequest { Value = 123 };

        var expectedException = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrows(autoMocker, fakeRequestA, expectedException);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();

        // The targeted request throws.
        var actualExceptionA = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await mockInteraction.Object.DispatchAsync(fakeRequestA));
        Assert.AreSame(expectedException, actualExceptionA);

        // The targeted request throws.
        var actualExceptionB = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await mockInteraction.Object.DispatchAsync(fakeRequestB));
        Assert.AreSame(expectedException, actualExceptionB);

        // Other requests do not throw.
        try {
            await mockInteraction.Object.DispatchAsync(new ExampleRequest {
                Value = 456,
            });
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion

    #region MockThrowsExactly<TRequest>(AutoMocker, TRequest, Exception)

    [TestMethod]
    public async Task MockThrowsExactly_RequestThrowsExpectedException()
    {
        var autoMocker = new AutoMocker();

        // Separate instances of the same request.
        var fakeRequestA = new ExampleRequest { Value = 123 };
        var fakeRequestB = new ExampleRequest { Value = 123 };

        var expectedException = new InvalidRequestException();
        InteractionAutoMockerExtensions.MockThrowsExactly(autoMocker, fakeRequestA, expectedException);

        var mockInteraction = autoMocker.GetMock<IInteractionDispatcher>();

        // The targeted request throws.
        var actualException = await Assert.ThrowsExactlyAsync<InvalidRequestException>(async ()
            => await mockInteraction.Object.DispatchAsync(fakeRequestA));
        Assert.AreSame(expectedException, actualException);

        // Other requests do not throw.
        try {
            await mockInteraction.Object.DispatchAsync(fakeRequestB);
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion
}
