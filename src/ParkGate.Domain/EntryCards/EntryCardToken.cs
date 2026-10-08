namespace ParkGate.Domain.EntryCards;

public sealed record EntryCardToken(byte KeyId, Guid CardId, DateOnly ExpiresOn);

public sealed record QrPublicKey(byte KeyId, byte[] SubjectPublicKeyInfo);

public enum QrVerifyStatus
{
    Valid,
    Unreadable,
    UnknownKey,
    InvalidSignature,
}

public readonly record struct QrVerifyResult(QrVerifyStatus Status, EntryCardToken? Token);
