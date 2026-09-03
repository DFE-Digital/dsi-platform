namespace Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

/// <summary>
/// Defines methods for hashing passwords and managing password hashing policies.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes <paramref name="rawPassword"/> using the given policy code and salt.
    /// </summary>
    string Hash(string policyCode, string rawPassword, string salt);

    /// <summary>
    /// Hashes using the latest policy code ("v4").
    /// </summary>
    string HashWithLatestPolicy(string rawPassword, string salt);

    /// <summary>
    /// Resolves the effective policy code for a user from their linked policies.#
    /// </summary>
    string ResolveUserPolicyCode(IEnumerable<string> userPolicyCodes);

    /// <summary>
    /// Generates a new random salt using the DfE Sign-in salt charset.
    /// </summary>
    string GenerateSalt();

    /// <summary>
    /// Gets the latest password hashing policy code.
    /// </summary>
    string LatestPolicyCode { get; }
}
