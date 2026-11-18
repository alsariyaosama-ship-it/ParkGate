namespace ParkGate.Application.Common;

/// <summary>
/// Thrown by <see cref="Ports.Persistence.IUnitOfWork.SaveChangesAsync"/> when another desk changed the same row
/// first (optimistic concurrency, ADR-026). Use cases do not catch it themselves; <see cref="ConcurrencyRetry"/> does.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Runs a use case body again from the start when it loses a concurrency race (ADR-026, R6.13).
/// The body must create its own unit of work, so every attempt reads fresh data and is evaluated by the normal
/// rules: the loser of two simultaneous check-ins sees the winner's entry and is denied by the cooldown.
/// </summary>
public static class ConcurrencyRetry
{
    public const int MaxAttempts = 3;

    public static async Task<Result<T>> RunAsync<T>(Func<CancellationToken, Task<Result<T>>> body, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await body(cancellationToken);
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                // Lost the race: run the whole use case again on fresh data.
            }
            catch (ConcurrencyConflictException)
            {
                return Errors.General.ConcurrencyConflict;
            }
        }
    }

    public static async Task<Result> RunAsync(Func<CancellationToken, Task<Result>> body, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await body(cancellationToken);
            }
            catch (ConcurrencyConflictException) when (attempt < MaxAttempts)
            {
                // Lost the race: run the whole use case again on fresh data.
            }
            catch (ConcurrencyConflictException)
            {
                return Errors.General.ConcurrencyConflict;
            }
        }
    }
}
