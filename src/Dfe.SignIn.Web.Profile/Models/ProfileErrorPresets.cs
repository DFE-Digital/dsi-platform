using Dfe.SignIn.WebFramework.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.Profile.Models;

public static class ProfileErrorPresets
{
    /// <summary>
    /// Profile details could not be loaded.
    /// </summary>
    public static ErrorViewModel ProfileLoadFailed(IUrlHelper url) => new() {
        PageTitle = "System error",
        Heading = "System error",
        Paragraphs = ["We couldn't load your profile right now. Please try again."],
        ActionButtonText = "Try again",
        ActionButtonUrl = url.Action(nameof(Controllers.HomeController.Index), "Home"),
        HelpLink = PersistServiceDeskHelp,
    };

    /// <summary>
    /// Job title update failed.
    /// </summary>
    public static ErrorViewModel JobTitleUpdateFailed(IUrlHelper url) => new() {
        PageTitle = "System error",
        Heading = "System error",
        Paragraphs = ["We couldn't change your job title right now. Please try again."],
        ActionButtonText = "Try again",
        ActionButtonUrl = url.Action(nameof(Controllers.ChangeJobTitleController.Index), "ChangeJobTitle"),
        HelpLink = PersistServiceDeskHelp,
    };

    private static LinkSentence PersistServiceDeskHelp { get; } = new(
        TextBefore: "If the problem persists ",
        LinkText: "contact our service desk team",
        Path: "contact-us"
    );

    /// <summary>
    /// Change name API failure (Node <c>changeNameError.ejs</c> parity).
    /// </summary>
    public static ErrorViewModel NameUpdateFailed(IUrlHelper url) => new() {
        PageTitle = "System error",
        Heading = "System error",
        Paragraphs = ["An error ocurred while trying to change your name, please try again."],
        ActionButtonText = "Try again",
        ActionButtonUrl = url.Action(nameof(Controllers.ChangeNameController.Index), "ChangeName"),
        HelpLink = PersistServiceDeskHelp,
        BackLink = new BackLinkViewModel {
            Href = new Uri(url.Action(nameof(Controllers.HomeController.Index), "Home") ?? "/", UriKind.Relative)
        }
    };

    /// <summary>
    /// Change email API failure (Node <c>entraChangeEmailError.ejs</c> parity).
    /// </summary>
    public static ErrorViewModel EmailUpdateFailed(IUrlHelper url) => new() {
        PageTitle = "System error",
        Heading = "System error",
        Paragraphs = ["An error ocurred while trying to change your email address, please try again."],
        ActionButtonText = "Try again",
        ActionButtonUrl = url.Action(nameof(Controllers.ChangeEmailController.Index), "ChangeEmail"),
        HelpLink = PersistServiceDeskHelp,
    };

    /// <summary>
    /// Change email succeeded but Entra MFA sync failed
    /// (Node <c>entraChangeEmailMfaError.ejs</c> parity).
    /// </summary>
    public static ErrorViewModel EmailMfaSyncFailed(IUrlHelper url) => new() {
        PageTitle = "System error",
        Heading = "System error",
        Paragraphs = [
            "Your email address has been updated, however, Multi-factor authentication (MFA) codes will still be sent to your old email address.",
        ],
        ActionButtonText = "Try again",
        ActionButtonUrl = url.Action(nameof(Controllers.ChangeEmailController.Index), "ChangeEmail"),
        HelpLink = new LinkSentence(
            TextBefore: "Please ",
            LinkText: "contact our service desk team",
            TextAfter: " so that they can resolve this.",
            Path: "contact-us"
        ),
    };

    /// <summary>
    /// Password change requires renewed associated-account authentication.
    /// </summary>
    public static ErrorViewModel PasswordAuthenticationRequired(IUrlHelper url) => new() {
        PageTitle = "Sign in again",
        Heading = "Sign in again",
        Paragraphs = ["Your password change session is no longer valid. Sign in again to change your password."],
        ActionButtonText = "Sign in again",
        ActionButtonUrl = url.Action(nameof(Controllers.ChangePasswordController.Index), "ChangePassword",
            new { reauthenticate = true }),
    };

    /// <summary>
    /// Unexpected password change failure
    /// (Node <c>entraPasswordChangeHandlerError.ejs</c> parity).
    /// </summary>
    /// <remarks>
    /// Used for unexpected Entra failures already handled in .NET.
    /// </remarks>
    public static ErrorViewModel PasswordUpdateFailed(IUrlHelper url) => new() {
        PageTitle = "System error",
        Heading = "System error",
        Paragraphs = ["An error ocurred while trying to change your password, please try again."],
        ActionButtonText = "Try again",
        ActionButtonUrl = url.Action(nameof(Controllers.ChangePasswordController.Index), "ChangePassword"),
        HelpLink = PersistServiceDeskHelp,
    };
}
