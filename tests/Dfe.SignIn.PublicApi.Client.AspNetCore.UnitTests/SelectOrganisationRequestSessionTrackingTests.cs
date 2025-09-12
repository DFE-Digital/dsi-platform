using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.AspNetCore.UnitTests;

[TestClass]
public sealed class SelectOrganisationRequestSessionTrackingTests
{
    private static readonly Guid FakeRequestId1 = new("27809199-89b2-4da1-a118-5b7284ac14cc");
    private static readonly Guid FakeRequestId2 = new("2e99d2f9-66a5-4b30-948a-bae0015928d7");

    private static void SetupMockHttpContext(AutoMocker autoMocker)
    {
        var mockContext = autoMocker.GetMock<HttpContext>();

        var mockSession = autoMocker.GetMock<ISession>();
        mockContext.Setup(x => x.Session).Returns(mockSession.Object);
    }

    #region SetTrackedRequestAsync(IHttpContext, Guid?)

    [TestMethod]
    public async Task SetTrackedRequestAsync_SetsString_WhenRequestIdIsProvided()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var tracking = new SelectOrganisationRequestSessionTracking();

        await tracking.SetTrackedRequestAsync(adapter, FakeRequestId1);

        autoMocker.Verify<ISession>(x =>
            x.Set(
                It.Is<string>(key => key == SelectOrganisationRequestSessionTracking.TrackedRequestIdSessionKey),
                It.Is<byte[]>(value => Encoding.UTF8.GetString(value) == FakeRequestId1.ToString())
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task SetTrackedRequestAsync_ClearsString_WhenRequestIdIsNull()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        var tracking = new SelectOrganisationRequestSessionTracking();

        await tracking.SetTrackedRequestAsync(adapter, FakeRequestId1);
        await tracking.SetTrackedRequestAsync(adapter, null);

        autoMocker.Verify<ISession>(x =>
            x.Remove(
                It.Is<string>(key => key == SelectOrganisationRequestSessionTracking.TrackedRequestIdSessionKey)
            ),
            Times.Once
        );
    }

    #endregion

    #region IsTrackingRequestAsync(IHttpContext, Guid)

    [TestMethod]
    public async Task IsTrackingRequestAsync_ReturnsTrue_WhenRequestMatchesTrackedRequest()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        byte[] output = Encoding.UTF8.GetBytes(FakeRequestId1.ToString());

        autoMocker.GetMock<ISession>()
            .Setup(x => x.TryGetValue(
                It.Is<string>(key => key == SelectOrganisationRequestSessionTracking.TrackedRequestIdSessionKey),
                out output!
            ))
            .Returns(true);

        var tracking = new SelectOrganisationRequestSessionTracking();

        bool result = await tracking.IsTrackingRequestAsync(adapter, FakeRequestId1);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsTrackingRequestAsync_ReturnsFalse_WhenRequestDoesNotMatchTrackedRequest()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        byte[] output = Encoding.UTF8.GetBytes(FakeRequestId1.ToString());

        autoMocker.GetMock<ISession>()
            .Setup(x => x.TryGetValue(
                It.Is<string>(key => key == SelectOrganisationRequestSessionTracking.TrackedRequestIdSessionKey),
                out output!
            ))
            .Returns(true);

        var tracking = new SelectOrganisationRequestSessionTracking();

        bool result = await tracking.IsTrackingRequestAsync(adapter, FakeRequestId2);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsTrackingRequestAsync_ReturnsFalse_WhenNoRequestWasBeingTracked()
    {
        var autoMocker = new AutoMocker();
        SetupMockHttpContext(autoMocker);
        var adapter = autoMocker.CreateInstance<HttpContextAspNetCoreAdapter>();

        byte[]? output = null;

        autoMocker.GetMock<ISession>()
            .Setup(x => x.TryGetValue(
                It.Is<string>(key => key == SelectOrganisationRequestSessionTracking.TrackedRequestIdSessionKey),
                out output
            ))
            .Returns(false);

        var tracking = new SelectOrganisationRequestSessionTracking();

        bool result = await tracking.IsTrackingRequestAsync(adapter, FakeRequestId2);

        Assert.IsFalse(result);
    }

    #endregion
}
