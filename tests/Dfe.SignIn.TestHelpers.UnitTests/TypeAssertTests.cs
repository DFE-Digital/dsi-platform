using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Dfe.SignIn.TestHelpers.UnitTests;

[TestClass]
public sealed class TypeAssertTests
{
    #region IsType<T>(object)

    [TestMethod]
    [ExpectedException(typeof(AssertFailedException))]
    public void IsType_Throws_WhenValueArgumentIsNull()
    {
        TypeAssert.IsType<string>(null);
    }

    [TestMethod]
    [ExpectedException(typeof(AssertFailedException))]
    public void IsType_Throws_WhenValueArgumentIsNotTheExpectedType()
    {
        TypeAssert.IsType<string>(42);
    }

    [TestMethod]
    public void IsType_DoesNotThrow_WhenValueArgumentIsTheExpectedType()
    {
        TypeAssert.IsType<string>("A valid value!");
    }

    [TestMethod]
    public void IsType_ReturnsTypeCastedValue()
    {
        string result = TypeAssert.IsType<string>("A valid value!");

        Assert.IsNotNull(result);
        Assert.AreEqual("A valid value!", result);
    }

    #endregion

    #region IsViewModelType<TViewModel>(IActionResult)

    private class FakeViewModelA
    {
    }

    private class FakeViewModelB : FakeViewModelA
    {
    }

    private class FakeViewModelC
    {
    }

    private static ViewResult CreateFakeViewResult(object viewModel)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var modelState = new ModelStateDictionary();
        return new ViewResult {
            ViewName = "FakeView",
            ViewData = new ViewDataDictionary(metadataProvider, modelState) {
                Model = viewModel,
            },
        };
    }

    [TestMethod]
    [ExpectedException(typeof(AssertFailedException))]
    public void IsViewModelType_Throws_WhenResultArgumentIsNull()
    {
        TypeAssert.IsViewModelType<FakeViewModelA>(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(AssertFailedException))]
    public void IsViewModelType_Throws_WhenResultArgumentIsNotViewResult()
    {
        var result = new EmptyResult();

        TypeAssert.IsViewModelType<FakeViewModelA>(result);
    }

    [TestMethod]
    [ExpectedException(typeof(AssertFailedException))]
    public void IsViewModelType_Throws_WhenViewModelIsNotTheExpectedType()
    {
        var result = CreateFakeViewResult(new FakeViewModelC());

        TypeAssert.IsViewModelType<FakeViewModelA>(result);
    }

    [TestMethod]
    public void IsViewModelType_DoesNotThrow_WhenViewModelIsTheExpectedType()
    {
        var result = CreateFakeViewResult(new FakeViewModelA());

        TypeAssert.IsViewModelType<FakeViewModelA>(result);
    }

    [TestMethod]
    public void IsViewModelType_DoesNotThrow_WhenViewModelInheritsTheExpectedType()
    {
        var result = CreateFakeViewResult(new FakeViewModelB());

        TypeAssert.IsViewModelType<FakeViewModelA>(result);
    }

    [TestMethod]
    public void IsViewModelType_ReturnsTypeCastedViewModel()
    {
        var expectedViewModel = new FakeViewModelB();
        var result = CreateFakeViewResult(expectedViewModel);

        var viewModel = TypeAssert.IsViewModelType<FakeViewModelA>(result);

        Assert.AreSame(expectedViewModel, viewModel);
    }

    #endregion
}
