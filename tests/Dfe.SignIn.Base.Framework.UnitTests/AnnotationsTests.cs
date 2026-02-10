namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class AnnotationsTests
{
    [TestMethod]
    public void ThrowsAttribute_ExceptionTypeIsInitialized()
    {
        var attribute = new ThrowsAttribute(typeof(UnexpectedException));

        Assert.AreEqual(typeof(UnexpectedException), attribute.ExceptionType);
    }

    [TestMethod]
    public void AssociatedResponseAttribute_ResponseTypeIsInitialized()
    {
        var attribute = new AssociatedResponseAttribute(typeof(Fakes.ExampleResponse));

        Assert.AreEqual(typeof(Fakes.ExampleResponse), attribute.ResponseType);
    }
}
