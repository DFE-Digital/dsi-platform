namespace Dfe.SignIn.WebFramework.Mvc.UnitTests;

[TestClass]
public sealed class MvcNamingTests
{
    private sealed class AnExampleController { }

    private sealed class AnExampleWithoutSuffix { }

    #region Controller<TController>()

    [TestMethod]
    public void Controller_ReturnsNameAndOmitsControllerSuffix_WhenSuffixPresent()
    {
        string name = MvcNaming.Controller<AnExampleController>();

        Assert.AreEqual("AnExample", name);
    }

    [TestMethod]
    public void Controller_ReturnsNameOfController_WhenSuffixNotPresent()
    {
        string name = MvcNaming.Controller<AnExampleWithoutSuffix>();

        Assert.AreEqual("AnExampleWithoutSuffix", name);
    }

    #endregion
}
