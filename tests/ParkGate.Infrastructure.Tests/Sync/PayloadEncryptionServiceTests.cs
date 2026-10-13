using Microsoft.Extensions.Logging.Abstractions;
using ParkGate.Domain.Entities;
using ParkGate.Infrastructure.Security;
using ParkGate.Infrastructure.Sync;
using Shouldly;

namespace ParkGate.Infrastructure.Tests.Sync;

public sealed class PayloadEncryptionServiceTests
{
    private readonly string _testKey = AesHelper.GenerateKey();

    [Fact]
    public void AesHelper_EncryptAndDecrypt_RoundTripsAccurately()
    {
        var original = "0912345678";
        var encrypted = AesHelper.Encrypt(original, _testKey);

        encrypted.ShouldNotBe(original);
        encrypted.ShouldNotBeNullOrEmpty();

        var decrypted = AesHelper.Decrypt(encrypted, _testKey);
        decrypted.ShouldBe(original);
    }

    [Fact]
    public void EncryptSensitiveFields_Beneficiary_EncryptsNationalIdAndPhoneOnly()
    {
        var service = new PayloadEncryptionService(_testKey, NullLogger<PayloadEncryptionService>.Instance);
        var json = "{\"FullName\":\"Ali\",\"Phone\":\"0912345678\",\"NationalId\":\"119901234567\",\"CardNumber\":\"CARD-1\"}";

        var result = service.EncryptSensitiveFields(nameof(Beneficiary), json);

        result.ShouldContain("\"FullName\":\"Ali\"");
        result.ShouldContain("\"CardNumber\":\"CARD-1\"");
        result.ShouldNotContain("\"Phone\":\"0912345678\"");
        result.ShouldNotContain("\"NationalId\":\"119901234567\"");

        // Parse and decrypt to verify
        var node = System.Text.Json.Nodes.JsonNode.Parse(result)!.AsObject();
        var encryptedPhone = node["Phone"]!.ToString();
        var encryptedNid = node["NationalId"]!.ToString();

        AesHelper.Decrypt(encryptedPhone, _testKey).ShouldBe("0912345678");
        AesHelper.Decrypt(encryptedNid, _testKey).ShouldBe("119901234567");
    }

    [Fact]
    public void EncryptSensitiveFields_NonSensitiveEntity_LeavesPayloadUnchanged()
    {
        var service = new PayloadEncryptionService(_testKey, NullLogger<PayloadEncryptionService>.Instance);
        var json = "{\"Code\":\"SSF\",\"NameAr\":\"صندوق التضامن\"}";

        var result = service.EncryptSensitiveFields(nameof(Organization), json);

        result.ShouldBe(json);
    }

    [Fact]
    public void EncryptSensitiveFields_WithoutAesKey_LeavesPayloadUnchanged()
    {
        var service = new PayloadEncryptionService(aesKey: null, NullLogger<PayloadEncryptionService>.Instance);
        var json = "{\"Phone\":\"0912345678\"}";

        var result = service.EncryptSensitiveFields(nameof(Beneficiary), json);

        result.ShouldBe(json);
    }
}
