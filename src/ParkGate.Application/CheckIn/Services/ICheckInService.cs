using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.Common;

namespace ParkGate.Application.CheckIn.Services;

public interface ICheckInService
{
    Task<Result<CheckInResultDto>> RecordEntryAsync(RecordEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result<CheckInResultDto>> OverrideEntryAsync(OverrideEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result> VoidEntryAsync(VoidEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result<TodayDeskSummaryDto>> GetTodayDeskEntriesAsync(int? workstationId = null, CancellationToken cancellationToken = default);
}
