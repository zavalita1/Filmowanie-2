using System.Security.Cryptography;
using System.Text;
using Filmowanie.Account.Interfaces;

namespace Filmowanie.Account.Helpers;

public sealed class HashHelper : IHashHelper
{
    private const int SaltCharLength = 16;

    public string GetHash(string secret, string saltSeed)
    {
        var salt = GetHashBytes(saltSeed).Take(SaltCharLength).ToArray();

        if (salt.Length < SaltCharLength)
            throw new InvalidOperationException("Salt is shorter than required!");

        var secretBytes = GetBytes(secret);
        var secretHash = GetHashBytes(secretBytes.Concat(salt).ToArray()).Concat(salt).ToArray();
        var result = GetString(secretHash);
        return result;
    }

    public bool DoesHashEqual(string expectedHash, string secret)
    {
        var expectedValueWithSaltBytes = GetBytes(expectedHash);
        var saltBytes = expectedValueWithSaltBytes.TakeLast(SaltCharLength).ToArray();
        var secretBytes = GetBytes(secret);
        var secretWithSaltBytes = GetHashBytes(secretBytes.Concat(saltBytes).ToArray());
        var expectedValue = GetString(secretWithSaltBytes.Concat(saltBytes).ToArray());
        var actualValue = GetString(expectedValueWithSaltBytes);

        return string.Equals(expectedValue, actualValue);
    }

    private static string GetString(byte[] hashBytes)
    {
        var sb = new StringBuilder();
        foreach (var hashByte in hashBytes)
        {
            sb.Append(hashByte.ToString("X2"));
        }

        var hash = sb.ToString();
        return hash;
    }

    private static byte[] GetHashBytes(string value)
    {
        var bytes = GetBytes(value);
        return GetHashBytes(bytes);
    }

    private static byte[] GetHashBytes(byte[] bytes)
    {
        var hashBytes = MD5.HashData(bytes);
        return hashBytes;
    }

    private static byte[] GetBytes(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return bytes;
    }
}