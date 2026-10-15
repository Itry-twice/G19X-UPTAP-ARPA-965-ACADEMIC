using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;

namespace PluriOne.AuthService.Services;

/// <summary>Hash y verificación de contraseñas con Argon2id.</summary>
public sealed class PasswordHasher
{
    public string Hash(string password)
    {
        var config = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing, // Argon2id
            Version = Argon2Version.Nineteen,
            TimeCost = 3,
            MemoryCost = 65536,
            Lanes = 4,
            Threads = 4,
            Password = Encoding.UTF8.GetBytes(password),
            Salt = RandomNumberGenerator.GetBytes(16),
            HashLength = 32
        };
        return Argon2.Hash(config);
    }

    public bool Verify(string password, string encodedHash)
    {
        try
        {
            return Argon2.Verify(encodedHash, password);
        }
        catch
        {
            return false;
        }
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
