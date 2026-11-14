using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ParkGate.Application.Security;

/// <summary>
/// Short-lived, single-use supervisor approvals (R3.2). The supervisor's credentials are checked once by
/// <c>IAuthService.AuthorizeSupervisorAsync</c>, which issues an opaque token; the use case consumes it and resolves
/// the approver from it. The UI therefore never passes a user id that a use case would trust. Tokens live in
/// memory only: they never outlive the desk process, and a restart simply asks the supervisor again.
/// </summary>
public sealed class SupervisorApprovalStore
{
    /// <summary>Long enough to type a reason after the password, short enough that an unattended approval lapses.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(2);

    private const int TokenBytes = 32;

    private readonly ConcurrentDictionary<string, Approval> _approvals = new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider;

    public SupervisorApprovalStore(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>Issues a token for an already verified supervisor, bound to the workstation that asked for it.</summary>
    public string Issue(int supervisorUserId, int? workstationId)
    {
        RemoveExpired();

        var token = Base64Url(RandomNumberGenerator.GetBytes(TokenBytes));
        _approvals[token] = new Approval(supervisorUserId, workstationId, _timeProvider.GetUtcNow() + Lifetime);
        return token;
    }

    /// <summary>
    /// Spends the token. It succeeds once, before expiry and on the same workstation; any attempt removes the token,
    /// so a token presented on the wrong desk cannot be retried.
    /// </summary>
    public bool TryConsume(string? token, int? workstationId, out int supervisorUserId)
    {
        supervisorUserId = 0;
        if (string.IsNullOrEmpty(token) || !_approvals.TryRemove(token, out var approval))
        {
            return false;
        }

        if (approval.ExpiresAt < _timeProvider.GetUtcNow() || approval.WorkstationId != workstationId)
        {
            return false;
        }

        supervisorUserId = approval.SupervisorUserId;
        return true;
    }

    private void RemoveExpired()
    {
        var now = _timeProvider.GetUtcNow();
        foreach (var (token, approval) in _approvals)
        {
            if (approval.ExpiresAt < now)
            {
                _approvals.TryRemove(token, out _);
            }
        }
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed record Approval(int SupervisorUserId, int? WorkstationId, DateTimeOffset ExpiresAt);
}
