using System.Security.Cryptography;
using ParkGate.Application.Ports.Security;

namespace ParkGate.Infrastructure.Security;

/// <summary>
/// PBKDF2 with SHA-256 password hasher per AGENTS.md §4.3 and DATABASE.md §3.6.
/// Format: PBKDF2$SHA256$&lt;iterations&gt;$&lt;saltB64&gt;$&lt;hashB64&gt;
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public const int CurrentIterations = 600_000;
    public const int SaltLength = 16;
    public const int HashLength = 32;
    private const string HeaderPrefix = "PBKDF2$SHA256";

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            CurrentIterations,
            HashAlgorithmName.SHA256,
            HashLength);

        return $"{HeaderPrefix}${CurrentIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash, out bool needsRehash)
    {
        needsRehash = false;

        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('$');
        if (parts.Length != 5 || parts[0] != "PBKDF2" || parts[1] != "SHA256")
        {
            return false;
        }

        if (!int.TryParse(parts[2], out var iterations) || iterations <= 0)
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(parts[3]);
            expectedHash = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length == 0 || expectedHash.Length == 0)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        var matches = CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        if (!matches)
        {
            return false;
        }

        needsRehash = iterations < CurrentIterations;
        return true;
    }
}
