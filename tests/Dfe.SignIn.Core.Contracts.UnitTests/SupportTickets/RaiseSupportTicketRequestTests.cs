using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SupportTickets;

namespace Dfe.SignIn.Core.Contracts.UnitTests.SupportTickets;

[TestClass]
public sealed class RaiseSupportTicketRequestTests
{
    private static readonly RaiseSupportTicketRequest FakeRequest = new() {
        FullName = "Alex Johnson",
        EmailAddress = "alex@example.com",
        OrganisationName = "Example Organisation",
        SubjectCode = "create-account",
        ApplicationName = "Example Service",
        Message = "A message.",
    };

    [TestMethod]
    [DataRow(null)]
    [DataRow("123456")]
    [DataRow("1234567")]
    [DataRow("12345678")]
    public void ValidationOk_WhenOrganisationUrnIsValid(string? validValue)
    {
        ValidationHelpers.ValidateAndExpectOk(
            FakeRequest with {
                OrganisationUrn = validValue,
            }
        );
    }

    [TestMethod]
    [DataRow("a")]
    [DataRow("12345")]
    [DataRow("123456789")]
    public void ValidationFails_WhenOrganisationUrnIsInvalid(string invalidValue)
    {
        var results = ValidationHelpers.ValidateAndExpectFailure(
            FakeRequest with {
                OrganisationUrn = invalidValue,
            }
        );

        Assert.HasCount(1, results);
        Assert.AreEqual("Enter a valid URN or UKPRN, if known", results[0].ErrorMessage);
        string[] expectedMembers = [nameof(RaiseSupportTicketRequest.OrganisationUrn)];
        CollectionAssert.AreEqual(expectedMembers, results[0].MemberNames.ToArray());
    }

    [TestMethod]
    public void ValidationFails_WhenCustomSummaryWasNotProvidedForOtherSubjectType()
    {
        var results = ValidationHelpers.ValidateAndExpectFailure(
            FakeRequest with {
                SubjectCode = "other",
                CustomSummary = null,
            }
        );

        Assert.HasCount(1, results);
        Assert.AreEqual("Enter a summary of your issue", results[0].ErrorMessage);
        string[] expectedMembers = [nameof(RaiseSupportTicketRequest.CustomSummary)];
        CollectionAssert.AreEqual(expectedMembers, results[0].MemberNames.ToArray());
    }
}
