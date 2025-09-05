using Dfe.SignIn.Base.Framework.UnitTests.Fakes;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class InteractorTests
{
    #region InvokeAsync(InteractionContext<TRequest>, CancellationToken)

    [TestMethod]
    public async Task InvokeAsync_UntypedMethodInvokesTypedMethod()
    {
        var fakeInteractor = new ExampleUseCase();

        var response = await fakeInteractor.InvokeAsync(new ExampleRequest());

        TypeAssert.IsType<ExampleResponse>(response);
    }

    #endregion
}
