using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Http;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Models;

[TestClass]
public sealed class FrameworkErrorPresetsTests
{
    [TestMethod]
    public void DefaultServerError_MatchesExpress500Copy()
    {
        var model = FrameworkErrorPresets.DefaultServerError();

        Assert.AreEqual("There has been an error", model.PageTitle);
        Assert.AreEqual("There has been an error", model.Heading);
        Assert.AreEqual(0, model.Paragraphs.Count);
        Assert.IsNotNull(model.HelpLink);
        Assert.AreEqual("If the problem continues, follow the link to ", model.HelpLink.TextBefore);
        Assert.AreEqual("submit a support request", model.HelpLink.LinkText);
        Assert.AreEqual(".", model.HelpLink.TextAfter);
        Assert.AreEqual("contact-us", model.HelpLink.Path);
        Assert.IsNull(model.HelpLink.Href);
        Assert.IsFalse(model.ShowRequestId);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, model.StatusCode);
        Assert.IsNull(model.ActionButtonText);
        Assert.IsNull(model.ActionButtonUrl);
    }

    [TestMethod]
    public void DefaultServerError_UsesProvidedStatusCode()
    {
        var model = FrameworkErrorPresets.DefaultServerError(StatusCodes.Status405MethodNotAllowed);

        Assert.AreEqual(StatusCodes.Status405MethodNotAllowed, model.StatusCode);
    }

    [TestMethod]
    public void NotFound_MatchesNodeNotFoundCopy()
    {
        var model = FrameworkErrorPresets.NotFound();

        Assert.AreEqual("Page not found", model.PageTitle);
        Assert.AreEqual("Page not found", model.Heading);
        CollectionAssert.AreEqual(
            new[] {
                "If you typed the web address, check it is correct.",
                "If you pasted the web address, check you copied the entire address.",
            },
            model.Paragraphs.ToArray());
        Assert.IsNotNull(model.HelpLink);
        Assert.AreEqual(
            "If the web address is correct or you selected a link or button, you can ",
            model.HelpLink.TextBefore);
        Assert.AreEqual("contact the helpdesk", model.HelpLink.LinkText);
        Assert.AreEqual(" to report the issue.", model.HelpLink.TextAfter);
        Assert.AreEqual("contact-us", model.HelpLink.Path);
        Assert.IsFalse(model.ShowRequestId);
        Assert.AreEqual(StatusCodes.Status404NotFound, model.StatusCode);
    }

    [TestMethod]
    public void NotAuthorised_MatchesNodeNotAuthorisedCopy()
    {
        var model = FrameworkErrorPresets.NotAuthorised();

        Assert.AreEqual("You are not authorised to view this information", model.PageTitle);
        Assert.AreEqual("You are not authorised to view this information", model.Heading);
        Assert.AreEqual("govuk-heading-m govuk-!-margin-top-8", model.HeadingClass);
        Assert.AreEqual(0, model.Paragraphs.Count);
        Assert.IsNull(model.HelpLink);
        Assert.IsFalse(model.ShowRequestId);
        Assert.AreEqual(StatusCodes.Status403Forbidden, model.StatusCode);
    }

    [TestMethod]
    public void NotAuthorised_UsesProvidedStatusCode()
    {
        var model = FrameworkErrorPresets.NotAuthorised(StatusCodes.Status401Unauthorized);

        Assert.AreEqual(StatusCodes.Status401Unauthorized, model.StatusCode);
    }
}
