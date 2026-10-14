namespace ParkGate.Application.Ports.Sync;

public interface IPayloadEncryptionService
{
    string EncryptSensitiveFields(string entityType, string payloadJson);
}
