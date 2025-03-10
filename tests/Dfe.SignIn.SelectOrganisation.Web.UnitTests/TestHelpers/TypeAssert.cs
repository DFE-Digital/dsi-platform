
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.SelectOrganisation.Web.UnitTests.TestHelpers;

public static class TypeAssert
{
    public static T IsType<T>(object? value)
    {
        Assert.IsInstanceOfType<T>(value);
        return (T)value;
    }

    public static T IsViewModelType<T>(IActionResult result)
    {
        var viewResult = IsType<ViewResult>(result);
        return IsType<T>(viewResult.Model);
    }
}
