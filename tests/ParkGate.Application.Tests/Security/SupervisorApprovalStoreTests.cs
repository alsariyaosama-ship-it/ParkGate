using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;

namespace ParkGate.Application.Tests.Security;

/// <summary>R3.2: a supervisor approval is a single-use token that expires and stays on the desk that asked for it.</summary>
public sealed class SupervisorApprovalStoreTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);

    private readonly ManualTimeProvider _clock = new(Start);
    private readonly SupervisorApprovalStore _sut;

    public SupervisorApprovalStoreTests()
    {
        _sut = new SupervisorApprovalStore(_clock);
    }

    [Fact]
    public void TryConsume_FreshTokenOnTheSameDesk_ReturnsTheSupervisor()
    {
        var token = _sut.Issue(supervisorUserId: 7, workstationId: 3);

        _sut.TryConsume(token, workstationId: 3, out var supervisorUserId).ShouldBeTrue();
        supervisorUserId.ShouldBe(7);
    }

    [Fact]
    public void TryConsume_SameTokenTwice_FailsTheSecondTime()
    {
        var token = _sut.Issue(7, 3);
        _sut.TryConsume(token, 3, out _).ShouldBeTrue();

        _sut.TryConsume(token, 3, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryConsume_AfterTwoMinutes_Fails()
    {
        var token = _sut.Issue(7, 3);
        _clock.Now = Start.AddMinutes(2).AddSeconds(1);

        _sut.TryConsume(token, 3, out _).ShouldBeFalse();
    }

    [Fact]
    public void TryConsume_OnAnotherDesk_FailsAndTheTokenIsSpent()
    {
        var token = _sut.Issue(7, 3);

        _sut.TryConsume(token, 4, out _).ShouldBeFalse();
        _sut.TryConsume(token, 3, out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("made-up-token")]
    public void TryConsume_UnknownToken_Fails(string? token)
    {
        _sut.TryConsume(token, 3, out _).ShouldBeFalse();
    }

    [Fact]
    public void Issue_TwoApprovals_ProduceDifferentUnguessableTokens()
    {
        var first = _sut.Issue(7, 3);
        var second = _sut.Issue(7, 3);

        first.ShouldNotBe(second);
        first.Length.ShouldBeGreaterThanOrEqualTo(43); // 32 random bytes, base64url
    }
}
