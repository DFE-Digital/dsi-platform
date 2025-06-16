using Dfe.SignIn.Web.Help.Content;

namespace Dfe.SignIn.Web.Help.UnitTests.Content;

[TestClass]
public sealed class TopicPathHelpersTests
{
    #region TopicPathToTitle(string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TopicPathToTitle_Throws_WhenTopicPathArgumentIsNull()
    {
        TopicPathHelpers.TopicPathToTitle(null!);
    }

    [DataRow("", TopicPathHelpers.DefaultTitle)]
    [DataRow("/", TopicPathHelpers.DefaultTitle)]
    [DataRow("/about", "About")]
    [DataRow("/my-account/change-password", "Change password")]
    [DataRow("/my-account/change-password/", "Change password")]
    [DataTestMethod]
    public void TopicPathToTitle_ReturnsExpectedTitle(string topicPath, string expectedTitle)
    {
        string title = TopicPathHelpers.TopicPathToTitle(topicPath);

        Assert.AreEqual(expectedTitle, title);
    }

    #endregion

    #region SlugFromPath(string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SlugFromPath_Throws_WhenTopicPathArgumentIsNull()
    {
        TopicPathHelpers.SlugFromPath(null!);
    }

    [DataRow("", null)]
    [DataRow("/", null)]
    [DataRow("/about", "about")]
    [DataRow("/my-account/change-password", "change-password")]
    [DataRow("/my-account/change-password/", "change-password")]
    [DataTestMethod]
    public void SlugFromPath_ReturnsExpectedSlug(string topicPath, string? expectedSlug)
    {
        string? slug = TopicPathHelpers.SlugFromPath(topicPath);

        Assert.AreEqual(expectedSlug, slug);
    }

    #endregion

    #region GetParentTopicPath(string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetParentTopicPath_Throws_WhenTopicPathArgumentIsNull()
    {
        TopicPathHelpers.GetParentTopicPath(null!);
    }

    [DataRow("", null)]
    [DataRow("/", null)]
    [DataRow("/about", "/")]
    [DataRow("/about/", "/about")]
    [DataRow("/my-account/change-password", "/my-account")]
    [DataTestMethod]
    public void GetParentTopicPath_ReturnsExpectedParentTopicPath(string topicPath, string? expectedParentTopicPath)
    {
        string? parentTopicPath = TopicPathHelpers.GetParentTopicPath(topicPath);

        Assert.AreEqual(expectedParentTopicPath, parentTopicPath);
    }

    #endregion

    #region ResolveTopicPath(string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ResolveTopicPath_Throws_WhenTopicPathArgumentIsNull()
    {
        TopicPathHelpers.ResolveTopicPath(null!);
    }

    [TestMethod]
    public void ResolveTopicPath_EnsureStartsWithSlashCharacter()
    {
        string resolvedTopicPath = TopicPathHelpers.ResolveTopicPath("path/without/leading-slash-character");

        Assert.AreEqual("/path/without/leading-slash-character", resolvedTopicPath);
    }

    [TestMethod]
    public void ResolveTopicPath_NormalizesSlashCharacters()
    {
        string resolvedTopicPath = TopicPathHelpers.ResolveTopicPath("\\path\\with\\wrong-slash-characters");

        Assert.AreEqual("/path/with/wrong-slash-characters", resolvedTopicPath);
    }

    [DataRow("/", "/")]
    [DataRow("/path/with/trailing-slash-character/", "/path/with/trailing-slash-character")]
    [DataTestMethod]
    public void ResolveTopicPath_RemovesTrailingSlashCharacter(string topicPath, string expectedResolvedTopicPath)
    {
        string resolvedTopicPath = TopicPathHelpers.ResolveTopicPath(topicPath);

        Assert.AreEqual(expectedResolvedTopicPath, resolvedTopicPath);
    }

    [TestMethod]
    public void ResolveTopicPath_RemovesTrailingIndexTopicName()
    {
        string resolvedTopicPath = TopicPathHelpers.ResolveTopicPath("/my-account/index");

        Assert.AreEqual("/my-account", resolvedTopicPath);
    }

    [DataRow("/my-account/index.md", "/my-account")]
    [DataRow("/my-account/changing-my-password.md", "/my-account/changing-my-password")]
    [DataTestMethod]
    public void ResolveTopicPath_RemovesTrailingFileExtension(string topicPath, string expecedResolvedTopicPath)
    {
        string resolvedTopicPath = TopicPathHelpers.ResolveTopicPath(topicPath);

        Assert.AreEqual(expecedResolvedTopicPath, resolvedTopicPath);
    }

    #endregion
}
