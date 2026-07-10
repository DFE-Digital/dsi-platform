using System.Security.Cryptography;

namespace Dfe.SignIn.Core.UseCases.Users;

public static class CodeGenerator
{
    public const string NumericCharset = "123456789";

    public const string DecCharset = "46789BCDFGHJKLMNPRSTVWXY";

    public const string FullCharset =
        "ABCDEFGHJKMNPQRSTWXYZabcdefghjkmnpqrstwxyz23456789-.<>!@%&*+_";

    private static double GetRandom()
    {
        // Equivalent to:
        // crypto.randomBytes(4).readUInt32LE() / 0x100000000

        uint value = (uint)RandomNumberGenerator.GetInt32(int.MaxValue);
        return value / 4294967296.0;
    }

    public static string Generate(int length = 8, string? charset = null)
    {
        charset ??= DecCharset;

        var secret = string.Empty;

        for (int i = 0 ; i < length ; i++) {
            secret += charset[(int)Math.Floor(GetRandom() * charset.Length)];
        }

        return secret;
    }
}
