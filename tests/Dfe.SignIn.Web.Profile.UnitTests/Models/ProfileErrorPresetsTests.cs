using Dfe.SignIn.Web.Profile.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace Dfe.SignIn.Web.Profile.UnitTests.Models;

[TestClass]
public sealed class ProfileErrorPresetsTests
{
    private static IUrlHelper CreateUrlHelper()
    {
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns<UrlActionContext>(context => $"/{context.Controller}/{context.Action}");
        return mockUrlHelper.Object;
    }

    [TestMethod]
    public void NameUpdateFailed_ReturnsExpectedErrorViewModel()
    {
        var viewModel = ProfileErrorPresets.NameUpdateFailed(CreateUrlHelper());

        Assert.AreEqual("System error", viewModel.PageTitle);
        Assert.AreEqual("System error", viewModel.Heading);
        CollectionAssert.AreEqual(
            new[] { "An error ocurred while trying to change your name, please try again." },
            viewModel.Paragraphs.ToArray());
        Assert.AreEqual("Try again", viewModel.ActionButtonText);
        Assert.AreEqual("/ChangeName/Index", viewModel.ActionButtonUrl);
        Assert.IsNotNull(viewModel.HelpLink);
        Assert.AreEqual("If the problem persists ", viewModel.HelpLink.TextBefore);
        Assert.AreEqual("contact our service desk team", viewModel.HelpLink.LinkText);
        Assert.AreEqual(".", viewModel.HelpLink.TextAfter);
        Assert.AreEqual("contact-us", viewModel.HelpLink.Path);
        Assert.IsFalse(viewModel.ShowRequestId);
        Assert.IsNotNull(viewModel.BackLink);
        Assert.AreEqual("/Home/Index", viewModel.BackLink.Href.ToString());
    }

    [TestMethod]
    public void EmailUpdateFailed_ReturnsExpectedErrorViewModel()
    {
        var viewModel = ProfileErrorPresets.EmailUpdateFailed(CreateUrlHelper());

        Assert.AreEqual("System error", viewModel.PageTitle);
        Assert.AreEqual("System error", viewModel.Heading);
        CollectionAssert.AreEqual(
            new[] { "An error ocurred while trying to change your email address, please try again." },
            viewModel.Paragraphs.ToArray());
        Assert.AreEqual("Try again", viewModel.ActionButtonText);
        Assert.AreEqual("/ChangeEmail/Index", viewModel.ActionButtonUrl);
        Assert.IsNotNull(viewModel.HelpLink);
        Assert.AreEqual("If the problem persists ", viewModel.HelpLink.TextBefore);
        Assert.AreEqual("contact our service desk team", viewModel.HelpLink.LinkText);
        Assert.IsNull(viewModel.BackLink);
        Assert.IsFalse(viewModel.ShowRequestId);
    }

    [TestMethod]
    public void EmailMfaSyncFailed_ReturnsExpectedErrorViewModel()
    {
        var viewModel = ProfileErrorPresets.EmailMfaSyncFailed(CreateUrlHelper());

        Assert.AreEqual("System error", viewModel.PageTitle);
        Assert.AreEqual("System error", viewModel.Heading);
        CollectionAssert.AreEqual(
            new[] {
                "Your email address has been updated, however, Multi-factor authentication (MFA) codes will still be sent to your old email address.",
            },
            viewModel.Paragraphs.ToArray());
        Assert.AreEqual("Try again", viewModel.ActionButtonText);
        Assert.AreEqual("/ChangeEmail/Index", viewModel.ActionButtonUrl);
        Assert.IsNotNull(viewModel.HelpLink);
        Assert.AreEqual("Please ", viewModel.HelpLink.TextBefore);
        Assert.AreEqual("contact our service desk team", viewModel.HelpLink.LinkText);
        Assert.AreEqual(" so that they can resolve this.", viewModel.HelpLink.TextAfter);
        Assert.IsNull(viewModel.BackLink);
        Assert.IsFalse(viewModel.ShowRequestId);
    }
}
