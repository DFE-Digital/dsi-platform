using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

/// <summary>
/// Provides functionality for hashing passwords and managing password hashing policies.
/// </summary>
/// <remarks>
/// This class is declared as <c>partial</c> to enable the C# compile-time source generator
/// for regular expressions (<see cref="GeneratedRegexAttribute"/>).
/// </remarks>
public sealed partial class PasswordHasher : IPasswordHasher
{
    private const string SaltCharset = "ABCDEFGHJKMNPQRSTWXYZabcdefghjkmnpqrstwxyz23456789-.><!@%&*+_";
    private const string LegacyPolicyCode = "v2";

    /// <inheritdoc/>
    public string LatestPolicyCode => "v4";

    private static readonly Dictionary<string, (int Iterations, int KeyLenBytes)> Policies = new() {
        ["v2"] = (10_000, 512),
        ["v3"] = (120_000, 512),
        ["v4"] = (210_000, 64),
    };

    /// <inheritdoc/>
    public string Hash(string policyCode, string rawPassword, string salt)
    {
        if (!Policies.TryGetValue(policyCode, out var policy)) {
            throw new ArgumentException($"Invalid policy code '{policyCode}'.", nameof(policyCode));
        }

        byte[] derivedKey = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(rawPassword),
            salt: Encoding.UTF8.GetBytes(salt),
            iterations: policy.Iterations,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: policy.KeyLenBytes
        );

        return Convert.ToBase64String(derivedKey);
    }

    /// <inheritdoc/>
    public string HashWithLatestPolicy(string rawPassword, string salt)
    {
        return this.Hash(this.LatestPolicyCode, rawPassword, salt);
    }

    /// <inheritdoc/>
    public string ResolveUserPolicyCode(IEnumerable<string>? userPolicyCodes)
    {
        if (userPolicyCodes == null) {
            return LegacyPolicyCode;
        }

        var versions = userPolicyCodes
            .Where(code => !string.IsNullOrEmpty(code) && PolicyVersionRegex().IsMatch(code))
            .Select(code => int.Parse(code[1..]))
            .ToList();

        if (versions.Count == 0) {
            return LegacyPolicyCode;
        }

        return $"v{Math.Max(2, versions.Max())}";
    }

    /// <inheritdoc/>
    public string GenerateSalt()
    {
        Span<char> buffer = stackalloc char[25];
        for (int i = 0; i < buffer.Length; i++) {
            buffer[i] = SaltCharset[RandomNumberGenerator.GetInt32(SaltCharset.Length)];
        }

        return new string(buffer);
    }

    /// <inheritdoc/>
    public bool IsAttemptingToReusePassword(
        string policyCode,
        string newPassword,
        IEnumerable<(string PasswordHash, string Salt)> passwordHistory)
    {
        if (passwordHistory == null) {
            return false;
        }

        foreach (var (historicalHash, historicalSalt) in passwordHistory) {
            string derivedKey = this.Hash(policyCode, newPassword, historicalSalt);

            byte[] derivedBytes = Encoding.UTF8.GetBytes(derivedKey);
            byte[] historyBytes = Encoding.UTF8.GetBytes(historicalHash);

            if (CryptographicOperations.FixedTimeEquals(derivedBytes, historyBytes)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Matches valid policy version codes (e.g. "v2", "v3", "v4").
    /// Uses compile-time source generation for optimal performance and zero runtime compilation overhead.
    /// </summary>
    [GeneratedRegex(@"^v[1-9][0-9]*$")]
    private static partial Regex PolicyVersionRegex();
}
