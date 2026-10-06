using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.Settings.Models;
using ParkGate.Application.Settings.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Settings management form (FR-ADM-03, DATABASE §5, SPEC §6).
/// Admin only.
/// </summary>
public partial class SettingsForm : RtlFormBase
{
    private readonly ISettingsService? _settingsService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<SettingsForm>? _logger;

    public SettingsForm() : this(null, null, null)
    {
    }

    public SettingsForm(
        ISettingsService? settingsService,
        INotifyService? notifyService,
        ILogger<SettingsForm>? logger)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_Settings;
        _lblHeader.Text = Strings.Title_Settings;

        _tabGeneral.Text = Strings.Tab_GeneralSettings;
        _lblParkName.Text = Strings.Label_ParkName;
        _lblTimeZone.Text = Strings.Label_TimeZone;

        _tabEntry.Text = Strings.Tab_EntrySettings;
        _lblCooldownHours.Text = Strings.Label_DefaultCooldownHours;
        _lblMaxGroupSize.Text = Strings.Label_MaxGroupSize;
        _lblDenialDedupMinutes.Text = Strings.Label_DenialDedupMinutes;
        _chkPrefillCompanions.Text = Strings.Label_PrefillCompanions;
        _chkDenialSound.Text = Strings.Label_DenialSound;

        _tabSecurity.Text = Strings.Tab_SecuritySettings;
        _lblMaxFailedLogins.Text = Strings.Label_MaxFailedLogins;
        _lblLockoutMinutes.Text = Strings.Label_LockoutMinutes;
        _lblAutoLockMinutes.Text = Strings.Label_AutoLockMinutes;

        _btnSave.Text = Strings.Btn_Save;
        _btnReload.Text = Strings.Btn_Refresh;
        _btnClose.Text = Strings.Btn_Close;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await LoadSettingsAsync();
        _btnSave.Click += async (_, _) => await SaveAsync();
        _btnReload.Click += async (_, _) => await LoadSettingsAsync();
        _btnClose.Click += (_, _) => Close();
    }

    public async Task LoadSettingsAsync()
    {
        if (_settingsService == null)
        {
            return;
        }

        try
        {
            var s = await _settingsService.GetSettingsAsync();

            _txtParkName.Text = s.ParkName;
            _txtTimeZone.Text = s.TimeZoneId;

            var cooldownHours = Math.Max(1, Math.Min(168, (int)Math.Round((double)s.DefaultCooldownMinutes / 60)));
            _numCooldownHours.Value = cooldownHours;
            _numMaxGroupSize.Value = Math.Max(1, Math.Min(50, s.MaxGroupSize));
            _numDenialDedupMinutes.Value = Math.Max(1, Math.Min(60, s.DenialDedupMinutes));
            _chkPrefillCompanions.Checked = s.PrefillCompanionsFromLastVisit;
            _chkDenialSound.Checked = s.DenialSoundEnabled;

            _numMaxFailedLogins.Value = Math.Max(3, Math.Min(20, s.MaxFailedLogins));
            _numLockoutMinutes.Value = Math.Max(1, Math.Min(120, s.LockoutMinutes));
            _numAutoLockMinutes.Value = Math.Max(0, Math.Min(240, s.AutoLockMinutes));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading settings.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }

    public async Task SaveAsync()
    {
        if (_settingsService == null)
        {
            return;
        }

        var parkName = _txtParkName.Text.Trim();
        var timeZoneId = _txtTimeZone.Text.Trim();

        if (string.IsNullOrWhiteSpace(parkName))
        {
            ShowWarning("اسم المنتزه مطلوب.");
            _tabSettings.SelectedTab = _tabGeneral;
            _txtParkName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            timeZoneId = "Libya Standard Time";
        }

        var cooldownMinutes = (int)_numCooldownHours.Value * 60;
        var maxGroupSize = (int)_numMaxGroupSize.Value;
        var denialDedupMinutes = (int)_numDenialDedupMinutes.Value;
        var prefillCompanions = _chkPrefillCompanions.Checked;
        var denialSound = _chkDenialSound.Checked;

        var maxFailedLogins = (int)_numMaxFailedLogins.Value;
        var lockoutMinutes = (int)_numLockoutMinutes.Value;
        var autoLockMinutes = (int)_numAutoLockMinutes.Value;

        var request = new UpdateSettingsRequest(
            cooldownMinutes,
            maxGroupSize,
            denialDedupMinutes,
            prefillCompanions,
            denialSound,
            maxFailedLogins,
            lockoutMinutes,
            autoLockMinutes,
            parkName,
            timeZoneId);

        using (new BusyScope(this, _btnSave, _btnReload))
        {
            try
            {
                var result = await _settingsService.UpdateSettingsAsync(request);
                if (result.IsSuccess)
                {
                    _notifyService?.Info(Strings.Msg_SaveSettingsSuccess);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    ShowWarning(ErrorMessages.GetMessage(result.Error));
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving settings.");
                ShowError(Strings.Msg_UnexpectedError + Environment.NewLine + ex.Message);
            }
        }
    }

    private void ShowWarning(string message)
    {
        if (_notifyService != null)
        {
            _notifyService.Warning(message);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.Title_Warning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }

    private void ShowError(string message)
    {
        if (_notifyService != null)
        {
            _notifyService.Error(message);
        }
        else
        {
            MessageBox.Show(
                message,
                Strings.Title_Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        }
    }
}
