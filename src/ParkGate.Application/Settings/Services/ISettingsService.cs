using ParkGate.Application.Common;
using ParkGate.Application.Settings.Models;

namespace ParkGate.Application.Settings.Services;

public interface ISettingsService
{
    Task<AppSettingsModel> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<Result> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken cancellationToken = default);
    void InvalidateCache();
}
