using Dfe.SignIn.WebFramework.Mvc.Models;

namespace Dfe.SignIn.WebFramework.Mvc.UnitTests.Models;

[TestClass]
public sealed class LinkSentenceTests
{
    [TestMethod]
    public void ResolveHref_UsesHref_WhenHrefIsProvided()
    {
        var link = new LinkSentence(
            TextBefore: "Before ",
            LinkText: "link",
            Href: "https://example.test/support");

        var href = link.ResolveHref(new Uri("https://help.test/"), "exceptionTraceId=abc");

        Assert.AreEqual("https://example.test/support?exceptionTraceId=abc", href);
    }

    [TestMethod]
    public void ResolveHref_PrefersHref_WhenBothHrefAndPathAreProvided()
    {
        var link = new LinkSentence(
            TextBefore: "Before ",
            LinkText: "link",
            Path: "contact-us",
            Href: "https://example.test/support");

        var href = link.ResolveHref(new Uri("https://help.test/"));

        Assert.AreEqual("https://example.test/support", href);
    }

    [TestMethod]
    public void ResolveHref_ResolvesPathAgainstBaseUrl_WhenHrefIsNotProvided()
    {
        var link = new LinkSentence(
            TextBefore: "Before ",
            LinkText: "link",
            Path: "contact-us");

        var href = link.ResolveHref(new Uri("https://help.test/"));

        Assert.AreEqual("https://help.test/contact-us", href);
    }

    [TestMethod]
    public void ResolveHref_AppendsQueryWithAmpersand_WhenHrefAlreadyHasQuery()
    {
        var link = new LinkSentence(
            TextBefore: "Before ",
            LinkText: "link",
            Href: "https://example.test/support?foo=bar");

        var href = link.ResolveHref(query: "exceptionTraceId=abc");

        Assert.AreEqual("https://example.test/support?foo=bar&exceptionTraceId=abc", href);
    }

    [TestMethod]
    public void ResolveHref_Throws_WhenNeitherHrefNorPathIsProvided()
    {
        var link = new LinkSentence(TextBefore: "Before ", LinkText: "link");

        Assert.ThrowsExactly<InvalidOperationException>(() => link.ResolveHref());
    }

    [TestMethod]
    public void ResolveHref_Throws_WhenPathIsUsedWithoutBaseUrl()
    {
        var link = new LinkSentence(
            TextBefore: "Before ",
            LinkText: "link",
            Path: "contact-us");

        Assert.ThrowsExactly<InvalidOperationException>(() => link.ResolveHref());
    }
}
