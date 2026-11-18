using ParkGate.Application.Common;
using ParkGate.Application.Maintenance.Services;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Domain.Enums;
using Shouldly;
using Xunit;

namespace ParkGate.Application.Tests.Maintenance;

public sealed class DatabaseSizeServiceTests
{
    private const long OneGiB = 1024L * 1024 * 1024;

    private readonly StubSizeQueries _queries = new();
    private readonly CurrentUserContext _user = new();

    private DatabaseSizeService CreateSut() => new(_queries, _user);

    [Theory]
    [InlineData(0L, 0, false)]
    [InlineData(OneGiB, 10, false)]
    [InlineData(7 * OneGiB - 1, 69, false)]
    [InlineData(7 * OneGiB, 70, true)]
    [InlineData(10 * OneGiB, 100, true)]
    public async Task GetAsync_ReportsPercentOfTheExpressLimit_AndWarnsFromSeventyPercent(long bytes, int percent, bool nearLimit)
    {
        _user.SetUser(1, "admin", "Admin", UserRole.Admin);
        _queries.Bytes = bytes;

        var result = await CreateSut().GetAsync();

        result.IsSuccess.ShouldBeTrue();
        result.Value.DataBytes.ShouldBe(bytes);
        result.Value.LimitBytes.ShouldBe(10 * OneGiB);
        result.Value.PercentUsed.ShouldBe(percent);
        result.Value.IsNearLimit.ShouldBe(nearLimit);
    }

    [Fact]
    public async Task GetAsync_WhenUserCannotTriggerBackup_ShouldBeForbidden()
    {
        _user.SetUser(2, "operator", "Operator", UserRole.Operator);

        var result = await CreateSut().GetAsync();

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
        _queries.Calls.ShouldBe(0);
    }

    private sealed class StubSizeQueries : IDatabaseSizeQueries
    {
        public long Bytes { get; set; }
        public int Calls { get; private set; }

        public Task<long> GetDataFilesSizeBytesAsync(CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(Bytes);
        }
    }
}
