using System.Security.Cryptography;

namespace Dfe.SignIn.InternalApi.Features.Users.UserCode;

/// <summary>
/// A utility class for generating random codes or secrets.
/// </summary>
public static class CodeGenerator
{
    /// <summary>
    /// The character set used for generating random codes, consisting of digits from 1 to 9.
    /// </summary>
    public const string NumericCharset = "123456789";

    /// <summary>
    /// The character set used for generating random codes, excluding easily confused characters such as '0', 'O', 'I', and 'l'.
    /// </summary>
    public const string DecCharset = "46789BCDFGHJKLMNPRSTVWXY";

    /// <summary>
    /// The full character set used for generating random codes, including uppercase letters, lowercase letters, digits, and special characters.
    /// </summary>
    public const string FullCharset =
        "ABCDEFGHJKMNPQRSTWXYZabcdefghjkmnpqrstwxyz23456789-.<>!@%&*+_";

    /// <summary>
    /// Generates a random string of the specified length using the provided character set.
    /// </summary>
    /// <param name="length">The length of the random string to generate.</param>
    /// <param name="charset">The character set to use for generating the random string.</param>
    /// <returns>A random string of the specified length.</returns>
    public static string Generate(int length = 8, string? charset = null)
    {
        charset ??= DecCharset;

        var secret = string.Empty;

        for (int i = 0 ; i < length ; i++) {
            secret += charset[(int)Math.Floor(GetRandom() * charset.Length)];
        }

        return secret;
    }

    private static double GetRandom()
    {
        // Equivalent to:
        // crypto.randomBytes(4).readUInt32LE() / 0x100000000

        byte[] bytes = new byte[4];
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(bytes);
        }
        uint value = BitConverter.ToUInt32(bytes, 0);
        return value / 4294967296.0;
    }
}
