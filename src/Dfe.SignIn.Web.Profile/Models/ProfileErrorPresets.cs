using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Models;

public static class ProfileErrorPresets
{
    /// <summary>
    /// Change name API failure (Node <c>changeNameError.ejs</c> parity).
    /// </summary>
    public static ErrorViewModel NameUpdateFailed(IUrlHelper url) => new() {
        PageTitle = "System error",
        Heading = "System error",
        Paragraphs = ["An error ocurred while trying to change your name, please try again."],
        ActionButtonText = "Try again",
        ActionButtonUrl = url.Action("Index", "ChangeName"),
        HelpLink = new LinkSentence(
            TextBefore: "If the problem persists ",
            LinkText: "contact our service desk team",
            Path: "contact-us"
        ),
        BackLink = new BackLinkViewModel {
            Href = new Uri(url.Action("Index", "Home") ?? "/", UriKind.Relative)
        }
    };
}
