using Dfe.SignIn.Web.Profile.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace Dfe.SignIn.Web.Profile.UnitTests.Models;

[TestClass]
public sealed class ProfileErrorPresetsTests
{
    [TestMethod]
    public void NameUpdateFailed_ReturnsExpectedErrorViewModel()
    {
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns<UrlActionContext>(context => $"/{context.Controller}/{context.Action}");

        var viewModel = ProfileErrorPresets.NameUpdateFailed(mockUrlHelper.Object);

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
}
