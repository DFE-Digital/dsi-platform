using System.Security.Cryptography;
using System.Text;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

/// <summary>
/// Provides functionality for hashing passwords and managing password hashing policies.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
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
    public string ResolveUserPolicyCode(IEnumerable<string> userPolicyCodes)
    {
        var versions = userPolicyCodes
            .Where(code => System.Text.RegularExpressions.Regex.IsMatch(code, "^v[1-9][0-9]*$"))
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
        for (int i = 0 ; i < buffer.Length ; i++) {
            buffer[i] = SaltCharset[RandomNumberGenerator.GetInt32(SaltCharset.Length)];
        }

        return new string(buffer);
    }
}
