using Dfe.SignIn.Base.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class InteractorTestExtensionsTests
{
    private sealed record FakeRequest { }
    private sealed record FakeResponse { }
    private sealed class FakeInteractor : Interactor<FakeRequest, FakeResponse>
    {
        public override Task<FakeResponse> InvokeAsync(InteractionContext<FakeRequest> context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new FakeResponse());
        }
    }

    #region HasInteractor<TRequest, TUseCase>(ServiceCollection)

    [TestMethod]
    public void HasInteractor_TInteractor_ReturnsTrue_WhenHasRegistration()
    {
        var services = new ServiceCollection();
        services.AddInteractor<FakeInteractor>();

        bool result = InteractorTestExtensions.HasInteractor<FakeRequest, FakeInteractor>(services);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasInteractor_TInteractor_ReturnsFalse_WhenRegistrationNotFound()
    {
        var services = new ServiceCollection();

        bool result = InteractorTestExtensions.HasInteractor<FakeRequest, FakeInteractor>(services);

        Assert.IsFalse(result);
    }

    #endregion

    #region HasInteractor<TRequest>(ServiceCollection)

    [TestMethod]
    public void HasInteractor_ReturnsTrue_WhenHasRegistration()
    {
        var services = new ServiceCollection();
        services.AddInteractor<FakeInteractor>();

        bool result = InteractorTestExtensions.HasInteractor<FakeRequest>(services);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasInteractor_ReturnsFalse_WhenRegistrationNotFound()
    {
        var services = new ServiceCollection();

        bool result = InteractorTestExtensions.HasInteractor<FakeRequest>(services);

        Assert.IsFalse(result);
    }

    #endregion
}
