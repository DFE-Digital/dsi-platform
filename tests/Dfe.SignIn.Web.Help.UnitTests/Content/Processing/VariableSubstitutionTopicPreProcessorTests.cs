using Dfe.SignIn.Web.Help.Content.Processing;

namespace Dfe.SignIn.Web.Help.UnitTests.Content.Processing;

[TestClass]
public sealed class VariableSubstitutionTopicPreProcessorTests
{
    [TestMethod]
    public async Task ProcessAsync_Throws_WhenVariableCannotBeResolved()
    {
        var processor = new VariableSubstitutionTopicPreProcessor(new());

        string topicPath = "/example-topic";
        string markdown = "Some value: ${{ UNEXPECTED_VARIABLE }}";

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => processor.ProcessAsync(topicPath, markdown)
        );

        Assert.AreEqual("Cannot resolve variable '${{ UNEXPECTED_VARIABLE }}' in topic '/example-topic'.", exception.Message);
    }

    [TestMethod]
    public async Task ProcessAsync_SubstitutesValues()
    {
        var processor = new VariableSubstitutionTopicPreProcessor(new() {
            Variables = {
                ["FIRST_VALUE"] = "Abc",
                ["SECOND_VALUE"] = "Def",
            },
        });

        string topicPath = "/example-topic";
        string markdown = "${{ FIRST_VALUE }} and ${{SECOND_VALUE}}";

        string result = await processor.ProcessAsync(topicPath, markdown);

        Assert.AreEqual("Abc and Def", result);
    }
}
