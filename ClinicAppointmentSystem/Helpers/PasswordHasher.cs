using System;
using System.Security.Cryptography;
using System.Text;

public static class PasswordHasher
{
    private const int SaltSize = 16; // 128-bit salt

    // Generates a hash + salt for a new password
    public static (string Hash, string Salt) HashPassword(string password)
    {
        // Generate random salt
        byte[] saltBytes = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        string saltBase64 = Convert.ToBase64String(saltBytes);

        // Hash password + salt
        string hashBase64 = ComputeHash(password, saltBytes);

        return (hashBase64, saltBase64);
    }

    // Verify entered password against stored hash + salt
    public static bool Verify(string password, string storedHash, string storedSalt)
    {
        byte[] saltBytes = Convert.FromBase64String(storedSalt);
        string hashOfInput = ComputeHash(password, saltBytes);
        return hashOfInput == storedHash;
    }

    private static string ComputeHash(string password, byte[] saltBytes)
    {
        using var sha256 = SHA256.Create();
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] combined = new byte[saltBytes.Length + passwordBytes.Length];

        Buffer.BlockCopy(saltBytes, 0, combined, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combined, saltBytes.Length, passwordBytes.Length);

        byte[] hashBytes = sha256.ComputeHash(combined);
        return Convert.ToBase64String(hashBytes);
    }
}
