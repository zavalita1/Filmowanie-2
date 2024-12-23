using System.Security.Cryptography;
using System.Text;
using Filmowanie.Account.Interfaces;

namespace Filmowanie.Account.Helpers;

internal sealed class HashHelper : IHashHelper
{
    private const int SaltCharLength = 16;

    public string GetHash(string secret, string saltSeed)
    {
        var saltEncoded = Encoding.UTF8.GetBytes(saltSeed);
        var salt = GetHashBytes(saltEncoded).Take(SaltCharLength).ToArray();

        var secretEncoded = Encoding.UTF8.GetBytes(secret);
        var secretWithSalt = secretEncoded.Concat(salt).ToArray();
        var secretHash = GetHashBytes(secretWithSalt).Concat(salt).ToArray();
        var result = GetString(secretHash);
        return result;
    }

    public bool DoesHashEqual(string expectedHash, string secret)
    {
        var expectedValueWithSaltBytes = GetBytes(expectedHash);
        var saltBytes = expectedValueWithSaltBytes.TakeLast(SaltCharLength).ToArray();
        var encodedSecret = Encoding.UTF8.GetBytes(secret);
        var secretWithSaltBytes = GetHashBytes(encodedSecret.Concat(saltBytes).ToArray());
        var expectedValue = GetString(secretWithSaltBytes.Concat(saltBytes).ToArray());
        var actualValue = GetString(expectedValueWithSaltBytes);

        return string.Equals(expectedValue, actualValue);
    }

    private static string GetString(byte[] hashBytes)
    {
        var result = Convert.ToHexString(hashBytes);
        return result;
    }

    private static byte[] GetHashBytes(byte[] bytes)
    {
        var hashBytes = MD5.HashData(bytes);
        return hashBytes;
    }

    private static byte[] GetBytes(string value)
    {
        var bytes = Convert.FromHexString(value);
        return bytes;
    }
}