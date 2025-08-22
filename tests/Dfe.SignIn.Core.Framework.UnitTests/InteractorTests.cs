using Dfe.SignIn.Core.Framework.UnitTests.Fakes;

namespace Dfe.SignIn.Core.Framework.UnitTests;

[TestClass]
public sealed class InteractorTests
{
    #region InvokeAsync(InteractionContext<TRequest>, CancellationToken)

    [TestMethod]
    public async Task InvokeAsync_UntypedMethodInvokesTypedMethod()
    {
        var fakeInteractor = new Example_UseCaseHandler();

        var response = await fakeInteractor.InvokeAsync(new ExampleRequest());

        TypeAssert.IsType<ExampleResponse>(response);
    }

    #endregion
}
