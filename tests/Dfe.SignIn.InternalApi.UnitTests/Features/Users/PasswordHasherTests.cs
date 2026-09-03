using Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

namespace Dfe.SignIn.InternalApi.UnitTests.Features.Users;

[TestClass]
public class PasswordHasherTests
{
    private readonly PasswordHasher hasher = new();

    [TestMethod]
    [DataRow("v2", "short password", "8780e6bb-8097-40a4-a60d-c5cc74ec9e52", "3Z1r8k+q0XQ6gWjYx1nJ8p5lL7mV3yG9zFfXK5Z1h2s=")]
    [DataRow("v3", "short password", "8780e6bb-8097-40a4-a60d-c5cc74ec9e52", "5Z2s9l+q1YR7hXkZx2oK9q6mM8nW4zH0aGgYL6Z2i3t=")]
    [DataRow("v4", "short password", "8780e6bb-8097-40a4-a60d-c5cc74ec9e52", "Te4cxIpW+CX7MS/zHNP00Kpui4Pml6LuBrBJTYiKnYITVqnmisOr8yBRgierIe8qofWCBIhh/3Ih/qdMLuQcxQ==")]
    public void Hash_MatchesKnownNodeVector(string policyCode, string password, string salt, string expected)
    {
        var result = this.hasher.Hash(policyCode, password, salt);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow(new string[0], "v2")]
    [DataRow(new[] { "v3" }, "v3")]
    [DataRow(new[] { "v2", "v3", "v4" }, "v4")]
    [DataRow(new[] { "v1" }, "v2")] // floors at 2
    public void ResolveUserPolicyCode_ReturnsHighestOrLegacyDefault(string[] codes, string expected)
    {
        Assert.AreEqual(expected, this.hasher.ResolveUserPolicyCode(codes));
    }
}

