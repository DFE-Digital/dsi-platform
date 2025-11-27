namespace Dfe.SignIn.Core.Contracts.UnitTests;

[TestClass]
public sealed class StringPatternsTests
{
    [TestMethod]
    [DataRow("Bob", true)]
    [DataRow("_Test", true)]
    [DataRow("Alex Johnson", true)]
    [DataRow("Alex-bob O'John-son", true)]
    [DataRow("", false)]
    [DataRow(" ", false)]
    [DataRow(" Bob ", false)]
    public void PersonNameRegex_WorksAsExpected(string input, bool expectedResult)
    {
        bool result = StringPatterns.PersonNameRegex().IsMatch(input);

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("bob@example.com", true)]
    [DataRow("alex-johnson@example.com", true)]
    [DataRow("alex-bob+o'john-son@sub-domain.example-a.com", true)]
    [DataRow("", false)]
    [DataRow(" ", false)]
    [DataRow(" bob@example.com ", false)]
    [DataRow("bob@bob@example.com ", false)]
    [DataRow("bob@bob ", false)]
    [DataRow("b%ob@example.com", false)]
    [DataRow(".bob@example.com", false)]
    [DataRow("bob@.example.com", false)]
    [DataRow("bob@example.com.", false)]
    public void EmailAddressRegex_WorksAsExpected(string input, bool expectedResult)
    {
        bool result = StringPatterns.EmailAddressRegex().IsMatch(input);

        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("", true)]
    [DataRow("Developer", true)]
    [DataRow("Software Developer", true)]
    [DataRow("Software Developer (and Tester)", true)]
    [DataRow("B2C Specailist", true)]
    [DataRow(" ", false)]
    [DataRow(" Developer ", false)]
    [DataRow("A & B", false)]
    public void JobTitleRegex_WorksAsExpected(string input, bool expectedResult)
    {
        bool result = StringPatterns.JobTitleRegex().IsMatch(input);

        Assert.AreEqual(expectedResult, result);
    }
}
