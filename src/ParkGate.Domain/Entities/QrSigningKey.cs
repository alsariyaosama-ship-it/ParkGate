namespace ParkGate.Domain.Entities;

/// <summary>
/// Public key of a desk that signs entry-card QR codes. The private key never leaves the desk (CNG store);
/// the database keeps only the public key so that every desk can verify cards issued by the others.
/// </summary>
public sealed class QrSigningKey
{
    public const int MaxPublicKeyLength = 200;
    public const int MaxStationNameLength = 100;

    public byte KeyId { get; private set; }
    public byte[] PublicKeySpki { get; private set; } = [];
    public string StationName { get; private set; } = string.Empty;

    /// <summary>Set by the database default (BR-05).</summary>
    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public bool IsRevoked => RevokedAtUtc.HasValue;

    private QrSigningKey() { }

    public static QrSigningKey Register(byte keyId, byte[] publicKeySpki, string stationName)
    {
        ArgumentNullException.ThrowIfNull(publicKeySpki);
        if (keyId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(keyId), "Key id starts at 1.");
        }

        if (publicKeySpki.Length == 0 || publicKeySpki.Length > MaxPublicKeyLength)
        {
            throw new ArgumentException($"Public key must be 1..{MaxPublicKeyLength} bytes.", nameof(publicKeySpki));
        }

        if (string.IsNullOrWhiteSpace(stationName))
        {
            throw new ArgumentException("Station name is required.", nameof(stationName));
        }

        var name = stationName.Trim();
        return new QrSigningKey
        {
            KeyId = keyId,
            PublicKeySpki = publicKeySpki.ToArray(),
            StationName = name.Length > MaxStationNameLength ? name[..MaxStationNameLength] : name,
        };
    }

    public void Revoke(DateTime nowUtc)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = nowUtc;
    }
}
