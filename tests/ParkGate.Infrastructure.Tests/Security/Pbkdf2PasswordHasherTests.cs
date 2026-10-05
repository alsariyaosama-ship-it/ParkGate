using System.Security.Cryptography;
using ParkGate.Infrastructure.Security;

namespace ParkGate.Infrastructure.Tests.Security;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ShouldReturnValidFormat()
    {
        var password = "SuperSecretPassword123!";
        var hash = _hasher.HashPassword(password);

        hash.ShouldStartWith("PBKDF2$SHA256$600000$");
        var parts = hash.Split('$');
        parts.Length.ShouldBe(5);
        parts[2].ShouldBe("600000");

        var salt = Convert.FromBase64String(parts[3]);
        salt.Length.ShouldBe(Pbkdf2PasswordHasher.SaltLength);

        var hashBytes = Convert.FromBase64String(parts[4]);
        hashBytes.Length.ShouldBe(Pbkdf2PasswordHasher.HashLength);
    }

    [Fact]
    public void HashPassword_TwiceOnSamePassword_ShouldUseDifferentSalts()
    {
        var password = "SuperSecretPassword123!";
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrueAndNoRehashNeeded()
    {
        var password = "CorrectPassword123!";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(password, hash, out var needsRehash);

        result.ShouldBeTrue();
        needsRehash.ShouldBeFalse();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
    {
        var password = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _hasher.HashPassword(password);

        var result = _hasher.VerifyPassword(wrongPassword, hash, out var needsRehash);

        result.ShouldBeFalse();
        needsRehash.ShouldBeFalse();
    }

    [Fact]
    public void VerifyPassword_WithLowerIterationCount_ShouldReturnTrueAndNeedsRehash()
    {
        var password = "LegacyPassword123!";
        var salt = RandomNumberGenerator.GetBytes(16);
        var legacyIterations = 100_000;
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            legacyIterations,
            HashAlgorithmName.SHA256,
            32);

        var legacyHash = $"PBKDF2$SHA256${legacyIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hashBytes)}";

        var result = _hasher.VerifyPassword(password, legacyHash, out var needsRehash);

        result.ShouldBeTrue();
        needsRehash.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid_hash")]
    [InlineData("PBKDF2$SHA1$1000$salt$hash")]
    [InlineData("PBKDF2$SHA256$notanumber$salt$hash")]
    [InlineData("PBKDF2$SHA256$1000$notbase64$notbase64")]
    public void VerifyPassword_WithMalformedHash_ShouldReturnFalse(string malformedHash)
    {
        var result = _hasher.VerifyPassword("password", malformedHash, out var needsRehash);

        result.ShouldBeFalse();
        needsRehash.ShouldBeFalse();
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldThrowArgumentException()
    {
        Should.Throw<ArgumentException>(() => _hasher.HashPassword(""));
    }
}
