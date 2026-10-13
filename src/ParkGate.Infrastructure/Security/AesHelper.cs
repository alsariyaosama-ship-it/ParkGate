using System.Security.Cryptography;

namespace ParkGate.Infrastructure.Security;

/// <summary>
/// AES-256-CBC encryption helper matching the cloud server encryption schema.
/// Payload format: Base64(16-byte IV + CipherText).
/// </summary>
public static class AesHelper
{
    public static string Encrypt(string plainText, string base64Key)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        byte[] keyBytes = Convert.FromBase64String(base64Key);
        if (keyBytes.Length != 32)
        {
            throw new ArgumentException("AES-256 key must be exactly 32 bytes (256 bits).", nameof(base64Key));
        }

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();

        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherTextBase64, string base64Key)
    {
        if (string.IsNullOrEmpty(cipherTextBase64))
        {
            return cipherTextBase64;
        }

        byte[] fullCipherBytes = Convert.FromBase64String(cipherTextBase64);
        byte[] keyBytes = Convert.FromBase64String(base64Key);

        if (keyBytes.Length != 32)
        {
            throw new ArgumentException("AES-256 key must be exactly 32 bytes (256 bits).", nameof(base64Key));
        }

        using var aes = Aes.Create();
        aes.Key = keyBytes;

        int ivLength = aes.BlockSize / 8; // 16 bytes
        if (fullCipherBytes.Length < ivLength)
        {
            throw new InvalidOperationException("Invalid ciphertext length.");
        }

        byte[] iv = new byte[ivLength];
        Array.Copy(fullCipherBytes, 0, iv, 0, ivLength);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(fullCipherBytes, ivLength, fullCipherBytes.Length - ivLength);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }

    public static string GenerateKey()
    {
        byte[] key = new byte[32];
        RandomNumberGenerator.Fill(key);
        return Convert.ToBase64String(key);
    }
}
