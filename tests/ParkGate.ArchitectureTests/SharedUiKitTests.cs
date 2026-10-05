using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Resources;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class SharedUiKitTests
{
    private static TControl GetField<TControl>(object target, string fieldName) where TControl : class
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.ShouldNotBeNull($"Field '{fieldName}' was not found on '{target.GetType().Name}'.");
        var val = field.GetValue(target) as TControl;
        val.ShouldNotBeNull($"Field '{fieldName}' could not be cast to '{typeof(TControl).Name}'.");
        return val;
    }

    [Fact]
    public void StatusBanner_InitialState_ShouldBeNeutralWithCorrectColor()
    {
        using var banner = new StatusBanner();

        banner.State.ShouldBe(BannerState.Neutral);
        banner.BackColor.ShouldBe(StatusBanner.ColorNeutral);
        banner.BackColor.ShouldBe(Color.FromArgb(69, 90, 100)); // #455A64 per AGENTS.md §5.10
        banner.Title.ShouldBe(Strings.Banner_Neutral);
    }

    [Fact]
    public void StatusBanner_SetGranted_ShouldHaveGrantedColorAndLargeFont()
    {
        using var banner = new StatusBanner();
        banner.SetGranted("مسموح بالدخول");

        banner.State.ShouldBe(BannerState.Granted);
        banner.BackColor.ShouldBe(StatusBanner.ColorGranted);
        banner.BackColor.ShouldBe(Color.FromArgb(27, 135, 63)); // #1B873F per AGENTS.md §5.10
        banner.Title.ShouldBe("مسموح بالدخول");

        var lblTitle = GetField<Label>(banner, "_lblTitle");
        lblTitle.Font.Size.ShouldBeGreaterThanOrEqualTo(20F); // >= 20 pt per SPEC §5.3 zone 3
    }

    [Fact]
    public void StatusBanner_SetDenied_ShouldHaveDeniedColorAndDetails()
    {
        using var banner = new StatusBanner();
        banner.SetDenied("مرفوض", "دخل قبل أقل من 12 ساعة");

        banner.State.ShouldBe(BannerState.Denied);
        banner.BackColor.ShouldBe(StatusBanner.ColorDenied);
        banner.BackColor.ShouldBe(Color.FromArgb(198, 40, 40)); // #C62828 per AGENTS.md §5.10
        banner.Title.ShouldBe("مرفوض");
        banner.Details.ShouldBe("دخل قبل أقل من 12 ساعة");

        var lblDetails = GetField<Label>(banner, "_lblDetails");
        lblDetails.Visible.ShouldBeTrue();
    }

    [Fact]
    public void StatusBanner_SetWarning_ShouldHaveWarningColor()
    {
        using var banner = new StatusBanner();
        banner.SetWarning("تنبيه");

        banner.State.ShouldBe(BannerState.Warning);
        banner.BackColor.ShouldBe(StatusBanner.ColorWarning);
        banner.BackColor.ShouldBe(Color.FromArgb(178, 106, 0)); // #B26A00 per AGENTS.md §5.10
    }

    [Fact]
    public void StatusBanner_Reset_ShouldRestoreNeutralState()
    {
        using var banner = new StatusBanner();
        banner.SetGranted("مسموح بالدخول");
        banner.Reset();

        banner.State.ShouldBe(BannerState.Neutral);
        banner.BackColor.ShouldBe(StatusBanner.ColorNeutral);
        banner.Title.ShouldBe(Strings.Banner_Neutral);
    }

    [Fact]
    public void CounterInput_InitialState_ShouldBeZero()
    {
        using var counter = new CounterInput();

        counter.Value.ShouldBe(0);
        counter.MinValue.ShouldBe(0);
        counter.MaxValue.ShouldBe(99);

        var txtValue = GetField<TextBox>(counter, "_txtValue");
        txtValue.Text.ShouldBe("0");

        var btnDecrement = GetField<Button>(counter, "_btnDecrement");
        btnDecrement.Enabled.ShouldBeFalse(); // Cannot decrement below 0

        var btnIncrement = GetField<Button>(counter, "_btnIncrement");
        btnIncrement.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void CounterInput_IncrementAndDecrement_ShouldChangeValueAndFireEvent()
    {
        using var counter = new CounterInput();
        var eventFiredCount = 0;
        counter.ValueChanged += (_, _) => eventFiredCount++;

        counter.Increment();
        counter.Value.ShouldBe(1);
        eventFiredCount.ShouldBe(1);

        counter.Increment();
        counter.Value.ShouldBe(2);
        eventFiredCount.ShouldBe(2);

        counter.Decrement();
        counter.Value.ShouldBe(1);
        eventFiredCount.ShouldBe(3);
    }

    [Fact]
    public void CounterInput_ArabicIndicDigits_ShouldBeNormalizedAutomatically()
    {
        using var counter = new CounterInput();
        var txtValue = GetField<TextBox>(counter, "_txtValue");

        // Simulate typing Arabic-Indic digits "٠٧" (7)
        txtValue.Text = "٠٧";

        counter.Value.ShouldBe(7);
        txtValue.Text.ShouldBe("07"); // Western ASCII digits
    }

    [Fact]
    public void CounterInput_Clamping_ShouldRespectMinAndMax()
    {
        using var counter = new CounterInput { MinValue = 0, MaxValue = 10 };

        counter.Value = 25;
        counter.Value.ShouldBe(10);

        counter.Value = -5;
        counter.Value.ShouldBe(0);
    }

    [Fact]
    public void BusyScope_ShouldDisableControlsAndRestoreOnDispose()
    {
        using var form = new Form();
        using var btn1 = new Button { Enabled = true };
        using var btn2 = new Button { Enabled = true };

        using (new BusyScope(form, btn1, btn2))
        {
            btn1.Enabled.ShouldBeFalse();
            btn2.Enabled.ShouldBeFalse();
            form.Cursor.ShouldBe(Cursors.WaitCursor);
        }

        btn1.Enabled.ShouldBeTrue();
        btn2.Enabled.ShouldBeTrue();
        form.Cursor.ShouldBe(Cursors.Default);
    }
}
