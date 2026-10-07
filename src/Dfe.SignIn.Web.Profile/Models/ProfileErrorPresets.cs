using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Models;

public static class ProfileErrorPresets
{
    public static ErrorViewModel NameUpdateFailed(IUrlHelper url) => new() {
        PageTitle = "System error",
        Heading = "System error",
        Paragraphs = ["An error ocurred while trying to change your name, please try again."],
        ActionButtonText = "Try again",
        ActionButtonUrl = url.Action("Index", "ChangeName"),
        HelpLinkText = "contact our service desk team",
        HelpLinkPrefix = "If the problem persists ",
        BackLink = new BackLinkViewModel {
            Href = new Uri(url.Action("Index", "Home") ?? "/", UriKind.Relative)
        }
    };
}
