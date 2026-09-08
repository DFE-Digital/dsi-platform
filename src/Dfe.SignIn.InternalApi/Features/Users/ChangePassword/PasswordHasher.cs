using System.Security.Cryptography;
using System.Text;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangePassword;

/// <summary>
/// Provides functionality for hashing passwords and managing password hashing policies.
/// </summary>
public sealed partial class PasswordHasher : IPasswordHasher
{
    // A carefully selected character set for salts that avoids ambiguous characters 
    // while providing high entropy.
    private const string SaltCharset = "ABCDEFGHJKMNPQRSTWXYZabcdefghjkmnpqrstwxyz23456789-.><!@%&*+_";

    // The fallback policy used for legacy users or when no policy history exists.
    private const string LegacyPolicyCode = "v2";

    /// <inheritdoc/>
    public string LatestPolicyCode => "v4";

    /// <summary>
    /// Maps a policy code to its specific PBKDF2 cryptographic parameters.
    /// Uses a switch expression for zero-allocation, highly optimised lookup.
    /// </summary>
    private static (int Iterations, int KeyLenBytes) GetPolicy(string policyCode) => policyCode switch {
        "v4" => (210_000, 64),
        "v3" => (120_000, 512),
        "v2" => (10_000, 512),
        _ => throw new ArgumentException($"Invalid policy code '{policyCode}'.", nameof(policyCode))
    };

    /// <inheritdoc/>
    public string Hash(string policyCode, string rawPassword, string salt)
    {
        var (iterations, keyLenBytes) = GetPolicy(policyCode);

        // PBKDF2 (Password-Based Key Derivation Function 2) applies a pseudorandom function 
        // to the input password along with a salt value and repeats the process many times 
        // to produce a derived key, preventing dictionary and brute-force attacks.
        byte[] derivedKey = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(rawPassword),
            salt: Encoding.UTF8.GetBytes(salt),
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: keyLenBytes
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
        if (userPolicyCodes is null) {
            return LegacyPolicyCode;
        }

        int maxVersion = -1;

        // Iterates through the list of policies associated with the user's history
        // to find the highest version number they have used.
        foreach (var code in userPolicyCodes) {
            if (string.IsNullOrEmpty(code) || !PasswordHasherRegex.PolicyVersionRegex().IsMatch(code)) {
                continue;
            }

            // Extracts the numeric version (e.g., "v3" -> 3) using a slice to avoid allocation
            if (int.TryParse(code.AsSpan(1), out int version) && version > maxVersion) {
                maxVersion = version;
            }
        }

        // Ensure we never downgrade below the legacy baseline (v2)
        return maxVersion < 2 ? LegacyPolicyCode : $"v{maxVersion}";
    }

    /// <inheritdoc/>
    public string GenerateSalt()
    {
        // string.Create is used here for high performance. It allocates the exact memory 
        // needed for the string on the heap once and writes directly to it using a Span,
        // avoiding intermediate array allocations.
        return string.Create(25, SaltCharset, static (span, charset) => {
            for (int i = 0 ; i < span.Length ; i++) {
                // Cryptographically secure random number generation for salt creation
                span[i] = charset[RandomNumberGenerator.GetInt32(charset.Length)];
            }
        });
    }

    /// <inheritdoc/>
    public bool IsAttemptingToReusePassword(string newPassword, IEnumerable<(string PolicyCode, string PasswordHash, string Salt)>? passwordHistory)
    {
        if (passwordHistory is null) {
            return false;
        }

        // Verify the new password against every previous password hash in the user's history.
        // It is critical that we use the specific historicalPolicyCode for each iteration,
        // otherwise the generated hashes won't match if the iterations/key lengths differ.
        foreach (var (historicalPolicyCode, historicalHash, historicalSalt) in passwordHistory) {
            var (iterations, keyLenBytes) = GetPolicy(historicalPolicyCode);

            byte[] derivedKeyBytes = Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(newPassword),
                salt: Encoding.UTF8.GetBytes(historicalSalt),
                iterations: iterations,
                hashAlgorithm: HashAlgorithmName.SHA512,
                outputLength: keyLenBytes
            );

            // Decode the stored base64 string back into raw bytes for a clean comparison
            byte[] historyBytes = Convert.FromBase64String(historicalHash);

            // FixedTimeEquals is strictly required here to prevent Timing Attacks. 
            // It ensures the comparison takes the exact same amount of time regardless 
            // of how many bytes match, masking the length of the matching prefix from attackers.
            if (CryptographicOperations.FixedTimeEquals(derivedKeyBytes, historyBytes)) {
                return true;
            }
        }

        return false;
    }
}
