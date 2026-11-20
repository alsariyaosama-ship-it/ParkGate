using System.Windows.Forms;
using ParkGate.WinForms.Services;
using Shouldly;

namespace ParkGate.ArchitectureTests;

/// <summary>R5.10 / FR-AUTH-06: the desk locks after the configured idle time.</summary>
public sealed class IdleMonitorTests
{
    private const int WmKeyDown = 0x0100;
    private const int WmMouseMove = 0x0200;
    private const int WmLButtonDown = 0x0201;
    private const int WmPaint = 0x000F;

    [Theory]
    [InlineData(0, 600, false)]  // 0 = auto-lock off
    [InlineData(5, 299, false)]
    [InlineData(5, 300, true)]
    [InlineData(5, 301, true)]
    public void IsIdle_AfterSeconds_DependsOnTheSetting(int minutes, int seconds, bool idle)
    {
        var clock = new ManualClock();
        var monitor = new IdleMonitor(clock);

        clock.Advance(TimeSpan.FromSeconds(seconds));

        monitor.IsIdle(minutes).ShouldBe(idle);
    }

    [Theory]
    [InlineData(WmKeyDown)]
    [InlineData(WmLButtonDown)]
    public void KeyOrClick_RestartsTheIdleTime(int message)
    {
        var clock = new ManualClock();
        var monitor = new IdleMonitor(clock);
        clock.Advance(TimeSpan.FromMinutes(4));

        var m = Message.Create(IntPtr.Zero, message, IntPtr.Zero, IntPtr.Zero);
        monitor.PreFilterMessage(ref m).ShouldBeFalse("the filter only watches; it never swallows input");
        clock.Advance(TimeSpan.FromMinutes(4));

        monitor.IsIdle(5).ShouldBeFalse();
    }

    [Fact]
    public void MouseMove_CountsOnlyWhenThePointerMoves()
    {
        var clock = new ManualClock();
        var monitor = new IdleMonitor(clock);
        var at = Message.Create(IntPtr.Zero, WmMouseMove, IntPtr.Zero, (IntPtr)((20 << 16) | 10));
        monitor.PreFilterMessage(ref at);
        clock.Advance(TimeSpan.FromMinutes(4));

        // Windows repeats WM_MOUSEMOVE at the same point (e.g. when a window appears under the pointer).
        var same = Message.Create(IntPtr.Zero, WmMouseMove, IntPtr.Zero, (IntPtr)((20 << 16) | 10));
        monitor.PreFilterMessage(ref same);
        clock.Advance(TimeSpan.FromMinutes(1));

        monitor.IsIdle(5).ShouldBeTrue();
    }

    [Fact]
    public void OtherMessages_DoNotCountAsInput()
    {
        var clock = new ManualClock();
        var monitor = new IdleMonitor(clock);
        clock.Advance(TimeSpan.FromMinutes(4));

        var paint = Message.Create(IntPtr.Zero, WmPaint, IntPtr.Zero, IntPtr.Zero);
        monitor.PreFilterMessage(ref paint);
        clock.Advance(TimeSpan.FromMinutes(1));

        monitor.IsIdle(5).ShouldBeTrue();
    }

    [Fact]
    public void Reset_AfterUnlock_StartsAgain()
    {
        var clock = new ManualClock();
        var monitor = new IdleMonitor(clock);
        clock.Advance(TimeSpan.FromMinutes(10));

        monitor.Reset();

        monitor.IsIdle(5).ShouldBeFalse();
    }

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 18, 10, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
