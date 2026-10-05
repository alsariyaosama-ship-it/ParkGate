namespace ParkGate.Application.Ports.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash, out bool needsRehash);
}
