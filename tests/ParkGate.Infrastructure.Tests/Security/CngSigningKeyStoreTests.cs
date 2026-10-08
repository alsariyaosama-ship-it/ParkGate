using System.Runtime.Versioning;
using System.Security.Cryptography;
using ParkGate.Infrastructure.EntryCards;
using Shouldly;
using Xunit;

namespace ParkGate.Infrastructure.Tests.Security;

[SupportedOSPlatform("windows")]
public sealed class CngSigningKeyStoreTests
{
    [Fact]
    public void OpenOrCreate_ShouldReturnFunctionalEcdsaKey()
    {
        var testKeyName = "ParkGate.TestKey." + Guid.NewGuid().ToString("N");
        try
        {
            using (var ecdsa = CngSigningKeyStore.OpenOrCreate(testKeyName))
            {
                ecdsa.ShouldNotBeNull();
                var spki = ecdsa.ExportSubjectPublicKeyInfo();
                spki.Length.ShouldBeGreaterThan(0);

                var data = new byte[] { 1, 2, 3, 4, 5 };
                var signature = ecdsa.SignData(data, HashAlgorithmName.SHA256);
                signature.Length.ShouldBeGreaterThan(0);
            }
        }
        finally
        {
            CngSigningKeyStore.Delete(testKeyName);
        }
    }
}
