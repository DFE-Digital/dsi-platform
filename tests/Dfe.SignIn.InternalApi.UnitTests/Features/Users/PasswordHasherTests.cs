using Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

namespace Dfe.SignIn.InternalApi.UnitTests.Features.Users;

[TestClass]
public class PasswordHasherTests
{
    private readonly PasswordHasher hasher = new();

    [TestMethod]
    [DataRow(
        "v2",
        "short password",
        "8780e6bb-8097-40a4-a60d-c5cc74ec9e52",
        "3HRdyE36Pk9gtVwTAtM0THn1qVUvhJolIXsjypb2muzNwHgcU4SpHXTqpmpkjfBLCD6T6tuRolrlcAdGxMZJ+To+8rmWnyvBQAPFS/tdRBhaGGexW7ddYc67CBYLA54ZiM+DEe1dglQQiAkxYM56A78pMf8+7G+7RXl12WjDtKNOQXCBXGHg9zQJpWXSlOcJ4KqH3F2M3zmjE7OjZOb041x77Gm1dDpghH6Rnh7+Npg9zTI3UHdtYH0kqw2VxQOZY0wdMy1Ij4yn0HEVV8RXSkxzLX8UJPSkRztBJysjiV2X3EOn3AiNQ0m/pWo4SSOXjSQdgM4A+ORxWD89nCKlsvwxCWvu+bTULMJqB5xIscuPS0OC/hPZmRXSiNa7uS9rF6sueJxYNYjdFr2OOhcmBYqj8RVZGlGBzgOqRl/0HSKBW5GzlCo0D7FfX/rbcDiNcYdY1Jh5bW0r+JGc9s2q1NKfvrkocv4i7c0IvnjJAOV5Q4uaT3YpZr+WsUgIMmp2NWMzcc9nJCesHB666jfe4XHAIL0+xUo/6EQBEipzC35DihtCoz3RdO85uVTANicBR7zXy4Oc4ZgVMKFsMj2NyXl+wf3tKjnIfEjnY64AFqriO9h8Eeervd14fEnz6CJ+YZyonqqIlrLhV9/Yzz+TPJW7vu14FhuD/Hw3Jvq66R0=")]
    [DataRow(
        "v3",
        "short password",
        "8780e6bb-8097-40a4-a60d-c5cc74ec9e52",
        "1cWELhY5s7WIDtizvJgpi1fMmTOzmr3bMwfAg+SllzXZAUx8yGuoboowTmQ+bu3kYO8giqiAudCVJe2bNQpcVJMHzdvzoamP0lwbWiiJemZCJjx7BfilLbysq7WKnjDysT6L/QC58pROfS4x44BBrKxIWWYBRvKbHnBUQqqjI/EOpEsLhrqoaSaXXzrosb/T4XtNpHr3oshJCp0R1P5PmaOGNCtWR9SVCNWSQRWz0swg5Tlpc6z/UZWmkydljgcJK9zZ3aQ0eD4WxA94gfdj1mM7UZJlKUOvhnpXf79QtUyyMW3+HcJ3R7/Js2FKz3MiSJJOh2NqTfoZwbx5sC0KjmOIjqiovMRRMAE+M0hIFpghjkcZyn7IgBqXnY4YD04KjGwxMoGEvGUzqGflyCtzIBbliB5j8wwhwevIJPqZYS9QqT2C9YtstSQ2vMzkJBHCdGxF+77M01V2E7AvWEZ27/m7Gb3y3gox31hzbK4HpQbKdOg4L9GHa1k5jiGmSujnLFBOomiD6xV62duOL5veiK9NUJm6Xj0Vf1q4TMuhMnag4Z8zsEtBSWi5OsIOXfjyItmTi2TlrFwXvUQK6m1cfAu2ZrQWuRti3CCah4P3qjNshhdsdmKFZp1zYZ1wRXkkcpCKuJgjCI7/X9cwsx7MdC5E+td4gF1vwMXQmkvh8VM=")]
    [DataRow(
        "v4",
        "short password",
        "8780e6bb-8097-40a4-a60d-c5cc74ec9e52",
        "Te4cxIpW+CX7MS/zHNP00Kpui4Pml6LuBrBJTYiKnYITVqnmisOr8yBRgierIe8qofWCBIhh/3Ih/qdMLuQcxQ==")]
    [DataRow(
        "v4",
        "example-password",
        "3d8e9a86-b919-4cc9-8151-06efa565e9f4",
        "pu/KIHS6C7R6sxkizzq72TDa18Ho/e8oszP/FdnCqRepuLgGCIwuP2O2vbWYbVvaiNt8FTFD4KIYsXVk2HuRKw==")]
    public void Hash_MatchesKnownNodeVector(string policyCode, string password, string salt, string expected)
    {
        var result = this.hasher.Hash(policyCode, password, salt);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void LatestPolicyCode_ReturnsV4()
    {
        Assert.AreEqual("v4", this.hasher.LatestPolicyCode);
    }

    [TestMethod]
    public void HashWithLatestPolicy_MatchesV4Hash()
    {
        const string password = "example-password";
        const string salt = "3d8e9a86-b919-4cc9-8151-06efa565e9f4";
        const string expected = "pu/KIHS6C7R6sxkizzq72TDa18Ho/e8oszP/FdnCqRepuLgGCIwuP2O2vbWYbVvaiNt8FTFD4KIYsXVk2HuRKw==";

        var result = this.hasher.HashWithLatestPolicy(password, salt);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow("v0")]
    [DataRow("v1")]
    [DataRow("v5")]
    [DataRow("invalid")]
    public void Hash_ThrowsArgumentException_WhenPolicyCodeIsInvalid(string invalidPolicyCode)
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            this.hasher.Hash(invalidPolicyCode, "password", "salt"));
    }

    [TestMethod]
    [DataRow(null, "v2")]
    [DataRow(new string[0], "v2")]
    [DataRow(new[] { "v3" }, "v3")]
    [DataRow(new[] { "v2", "v3", "v4" }, "v4")]
    [DataRow(new[] { "v2", "v4", "v3" }, "v4")] // preserves max regardless of ordering
    [DataRow(new[] { "v0" }, "v2")]              // floors at 2
    [DataRow(new[] { "v1" }, "v2")]              // floors at 2
    [DataRow(new[] { "invalid", "v3" }, "v3")]   // ignores non-policy strings
    [DataRow(new string[] { null!, "v3" }, "v3")] // handles null items safely
    public void ResolveUserPolicyCode_ReturnsHighestOrLegacyDefault(string[]? codes, string expected)
    {
        Assert.AreEqual(expected, this.hasher.ResolveUserPolicyCode(codes));
    }

    [TestMethod]
    public void GenerateSalt_Returns25CharacterStringFromValidCharset()
    {
        const string expectedCharset = "ABCDEFGHJKMNPQRSTWXYZabcdefghjkmnpqrstwxyz23456789-.><!@%&*+_";

        var salt = this.hasher.GenerateSalt();

        Assert.AreEqual(25, salt.Length);
        Assert.IsTrue(salt.All(c => expectedCharset.Contains(c)));

        // Generates different salts across calls
        var secondSalt = this.hasher.GenerateSalt();
        Assert.AreNotEqual(salt, secondSalt);
    }

    [TestMethod]
    public void IsAttemptingToReusePassword_ReturnsTrue_WhenPasswordMatchesHistory()
    {
        var history = new List<(string PasswordHash, string Salt)>
        {
            ("Eg8WRPb/61Jjgh3kIMs4iu3E18BYjUZuWpAA97oOjw5lljq2g7we/TlcpR49HZYBm/Ot6QKnUwnRVJ7KFCaiIg==", "8217a892-03af-463d-a878-a6fbb9e64b9a"),
            ("pu/KIHS6C7R6sxkizzq72TDa18Ho/e8oszP/FdnCqRepuLgGCIwuP2O2vbWYbVvaiNt8FTFD4KIYsXVk2HuRKw==", "3d8e9a86-b919-4cc9-8151-06efa565e9f4"),
            ("Fl7I5Fifvlq+20OC6GUDnhofWK1jS1iE6Uru2QhmVVNq8fNAHcVQmdtpzCrfhOwAXrKloSacwWMQOhB8BUhEAg==", "13e4a2b6-1833-4d58-97ff-90451284e59a"),
        };

        var result = this.hasher.IsAttemptingToReusePassword("v4", "example-password", history);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsAttemptingToReusePassword_ReturnsFalse_WhenPasswordDoesNotMatchHistory()
    {
        var history = new List<(string PasswordHash, string Salt)>
        {
            ("Eg8WRPb/61Jjgh3kIMs4iu3E18BYjUZuWpAA97oOjw5lljq2g7we/TlcpR49HZYBm/Ot6QKnUwnRVJ7KFCaiIg==", "8217a892-03af-463d-a878-a6fbb9e64b9a"),
            ("pu/KIHS6C7R6sxkizzq72TDa18Ho/e8oszP/FdnCqRepuLgGCIwuP2O2vbWYbVvaiNt8FTFD4KIYsXVk2HuRKw==", "3d8e9a86-b919-4cc9-8151-06efa565e9f4"),
        };

        var result = this.hasher.IsAttemptingToReusePassword("v4", "a-different-password", history);

        Assert.IsFalse(result);
    }
}
