using ParkGate.Domain.Entities;

namespace ParkGate.Application.Ports.Persistence;

public interface IWorkstationRepository
{
    Task<Workstation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Workstation?> GetByMachineNameAsync(string machineName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workstation>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Workstation workstation);
    void Update(Workstation workstation);
}
