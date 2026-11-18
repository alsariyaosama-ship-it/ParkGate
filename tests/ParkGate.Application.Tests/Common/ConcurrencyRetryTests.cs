using ParkGate.Application.Common;

namespace ParkGate.Application.Tests.Common;

/// <summary>R6.13 / ADR-026: a use case that loses a concurrency race runs again from the start, at most three times.</summary>
public sealed class ConcurrencyRetryTests
{
    [Fact]
    public async Task RunAsync_NoConflict_RunsOnce()
    {
        var calls = 0;

        var result = await ConcurrencyRetry.RunAsync<int>(_ => Task.FromResult<Result<int>>(++calls), CancellationToken.None);

        result.Value.ShouldBe(1);
        calls.ShouldBe(1);
    }

    [Fact]
    public async Task RunAsync_TwoConflictsThenSuccess_ReturnsTheThirdAttempt()
    {
        var calls = 0;

        var result = await ConcurrencyRetry.RunAsync<int>(_ =>
        {
            calls++;
            return calls < 3 ? throw new ConcurrencyConflictException("lost") : Task.FromResult<Result<int>>(calls);
        }, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(3);
    }

    [Fact]
    public async Task RunAsync_ConflictOnEveryAttempt_ReturnsConcurrencyConflictAfterThreeTries()
    {
        var calls = 0;

        var result = await ConcurrencyRetry.RunAsync(_ =>
        {
            calls++;
            throw new ConcurrencyConflictException("lost");
#pragma warning disable CS0162 // keeps the lambda typed as Func<CancellationToken, Task<Result>>
            return Task.FromResult(Result.Success());
#pragma warning restore CS0162
        }, CancellationToken.None);

        calls.ShouldBe(ConcurrencyRetry.MaxAttempts);
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(Errors.General.ConcurrencyConflict.Code);
    }

    [Fact]
    public async Task RunAsync_OtherException_IsNotRetried()
    {
        var calls = 0;

        await Should.ThrowAsync<InvalidOperationException>(() => ConcurrencyRetry.RunAsync<int>(_ =>
        {
            calls++;
            throw new InvalidOperationException("bug");
        }, CancellationToken.None));

        calls.ShouldBe(1);
    }
}
