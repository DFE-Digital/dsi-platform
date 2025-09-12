using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Base.Framework.UnitTests;

[TestClass]
public sealed class ValidationHelpersTests
{
    private sealed record ExampleModel
    {
        [MinLength(1, ErrorMessage = "Specify at least one character")]
        public string Value { get; set; } = "";
    }

    #region ValidateAndExpectFailure(object)

    [TestMethod]
    public void ValidateAndExpectFailure_ThrowsWhenValidationPasses()
    {
        var model = new ExampleModel { Value = "valid" };

        var exception = Assert.ThrowsExactly<UnexpectedException>(()
            => ValidationHelpers.ValidateAndExpectFailure(model));
        Assert.AreEqual("Expected validation to fail; but it passed!", exception.Message);
    }

    [TestMethod]
    public void ValidateAndExpectFailure_ReturnsListOfValidationResults()
    {
        var model = new ExampleModel();

        var results = ValidationHelpers.ValidateAndExpectFailure(model);

        Assert.HasCount(1, results);
        Assert.AreEqual("Specify at least one character", results[0].ErrorMessage);
        string[] expectedMemberNames = [nameof(ExampleModel.Value)];
        CollectionAssert.AreEqual(expectedMemberNames, results[0].MemberNames.ToArray());
    }

    #endregion

    #region ValidateAndExpectOk(object)

    [TestMethod]
    public void ValidateAndExpectOk_ThrowsWhenValidationFails()
    {
        var model = new ExampleModel();

        var exception = Assert.ThrowsExactly<UnexpectedException>(()
            => ValidationHelpers.ValidateAndExpectOk(model));
        Assert.AreEqual("Expected validation to pass; but it failed!", exception.Message);
    }

    [TestMethod]
    public void ValidateAndExpectOk_DoesNotThrow_WhenValidationPasses()
    {
        var model = new ExampleModel { Value = "valid" };

        try {
            ValidationHelpers.ValidateAndExpectOk(model);
        }
        catch (Exception ex) {
            Assert.Fail($"Expected no exception, but got: {ex.GetType().Name} - {ex.Message}");
        }
    }

    #endregion

    #region HasExactlyOneMember(ValidationContext, string[])

    private sealed record ExampleModelWithMultipleMembers
    {
        public string? First { get; set; }
        public string? Second { get; set; }
        public string? Third { get; set; }
    }

    private static readonly string[] ExampleModelWithMultipleMembersMemberNames
        = [
            nameof(ExampleModelWithMultipleMembers.First),
            nameof(ExampleModelWithMultipleMembers.Second),
            nameof(ExampleModelWithMultipleMembers.Third),
        ];

    [TestMethod]
    public void HasExactlyOneMember_ReturnsTrue_WhenExactlyOneMemberWasGiven()
    {
        var validationContext = new ValidationContext(
            new ExampleModelWithMultipleMembers {
                Second = "Test",
            }
        );

        bool result = ValidationHelpers.HasExactlyOneMember(
            validationContext,
            ExampleModelWithMultipleMembersMemberNames
        );

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasExactlyOneMember_ReturnsFalse_WhenNoMembersWereGiven()
    {
        var validationContext = new ValidationContext(
            new ExampleModelWithMultipleMembers()
        );

        bool result = ValidationHelpers.HasExactlyOneMember(
            validationContext,
            ExampleModelWithMultipleMembersMemberNames
        );

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasExactlyOneMember_ReturnsFalse_WhenMultipleMembersWereGiven()
    {
        var validationContext = new ValidationContext(
            new ExampleModelWithMultipleMembers {
                First = "Test1",
                Third = "Test2",
            }
        );

        bool result = ValidationHelpers.HasExactlyOneMember(
            validationContext,
            ExampleModelWithMultipleMembersMemberNames
        );

        Assert.IsFalse(result);
    }

    #endregion
}
