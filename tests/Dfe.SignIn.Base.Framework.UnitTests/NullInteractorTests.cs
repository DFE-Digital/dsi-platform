using Dfe.SignIn.Base.Framework.UnitTests.Fakes;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class NullInteractorTests
{
    [TestMethod]
    public async Task InvokeAsync_UntypedMethodInvokesTypedMethod()
    {
        var nullInteractor = new NullInteractor<ExampleRequest, ExampleResponse>();

        var response = await nullInteractor.InvokeAsync(new ExampleRequest());

        TypeAssert.IsType<ExampleResponse>(response);
    }
}
