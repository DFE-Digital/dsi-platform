using Moq;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class InteractionLimiterExtensionsTests
{
    private sealed record FakeRequest : IKeyedRequest
    {
        public string Key => "FakeKey";
    }

    #region LimitAndThrowAsync(IInteractionLimiter, IKeyedRequest)

    [TestMethod]
    public async Task LimitAndThrowAsync_Throws_WhenLimiterArgumentIsNull()
    {
        var fakeRequest = new FakeRequest();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(()
            => InteractionLimiterExtensions.LimitAndThrowAsync(null!, fakeRequest));
    }

    [TestMethod]
    public async Task LimitAndThrowAsync_Throws_WhenInteractionIsRejected()
    {
        var fakeRequest = new FakeRequest();

        var mockLimiter = new Mock<IInteractionLimiter>();
        mockLimiter
            .Setup(x => x.LimitActionAsync(
                It.Is<IKeyedRequest>(r => ReferenceEquals(r, fakeRequest))
            ))
            .ReturnsAsync(new InteractionLimiterResult { WasRejected = true });

        var exception = await Assert.ThrowsExactlyAsync<InteractionRejectedByLimiterException>(()
            => InteractionLimiterExtensions.LimitAndThrowAsync(mockLimiter.Object, fakeRequest));
        Assert.AreEqual("FakeRequest", exception.RequestTypeName);
        Assert.AreEqual("FakeKey", exception.RequestKey);
    }

    [TestMethod]
    public async Task LimitAndThrowAsync_DoesNotThrow_WhenInteractionIsNotRejected()
    {
        var fakeRequest = new FakeRequest();

        var mockLimiter = new Mock<IInteractionLimiter>();
        mockLimiter
            .Setup(x => x.LimitActionAsync(
                It.Is<IKeyedRequest>(r => ReferenceEquals(r, fakeRequest))
            ))
            .ReturnsAsync(new InteractionLimiterResult { WasRejected = false });

        try {
            await InteractionLimiterExtensions.LimitAndThrowAsync(mockLimiter.Object, fakeRequest);
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion
}
