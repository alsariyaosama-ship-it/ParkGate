using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Settings.Services;

namespace ParkGate.Application.Common;

/// <summary>
/// The park's business date from the database clock (BR-05), converted with <c>Park.TimeZoneId</c>. Use it
/// wherever a rule compares against "today", so a desk with a wrong Windows clock or time zone cannot change
/// the outcome (R3.7).
/// </summary>
public sealed class ParkClock
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly ISettingsService _settings;

    public ParkClock(IUnitOfWorkFactory uowFactory, ISettingsService settings)
    {
        _uowFactory = uowFactory;
        _settings = settings;
    }

    public async Task<DateOnly> GetBusinessDateAsync(CancellationToken cancellationToken = default)
    {
        var timeZoneId = (await _settings.GetSettingsAsync(cancellationToken)).TimeZoneId;

        await using var uow = _uowFactory.Create();
        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);

        return TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);
    }
}
