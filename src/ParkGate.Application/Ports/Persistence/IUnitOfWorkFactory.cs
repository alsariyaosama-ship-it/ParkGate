namespace ParkGate.Application.Ports.Persistence;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
