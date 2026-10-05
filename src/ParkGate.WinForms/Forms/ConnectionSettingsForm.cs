using System.Drawing;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Ports.Security;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;

namespace ParkGate.WinForms.Forms;

public partial class ConnectionSettingsForm : RtlFormBase
{
    private readonly IConnectionSettingsStore? _settingsStore;
    private readonly IDatabaseStartupService? _startupService;
    private readonly IAppConnectionStringProvider? _connectionStringProvider;

    public ConnectionSettingsForm() : this(null, null, null)
    {
    }

    public ConnectionSettingsForm(
        IConnectionSettingsStore? settingsStore,
        IDatabaseStartupService? startupService,
        IAppConnectionStringProvider? connectionStringProvider)
    {
        InitializeComponent();

        _settingsStore = settingsStore;
        _startupService = startupService;
        _connectionStringProvider = connectionStringProvider;

        InitializeFormTextAndOptions();
        PopulateFromExistingSettings();
        WireEvents();
    }

    private void InitializeFormTextAndOptions()
    {
        Text = Strings.Title_ConnectionSettings;
        lblHeader.Text = Strings.Title_ConnectionSettings;
        lblServer.Text = Strings.Label_Server;
        lblDatabase.Text = Strings.Label_Database;
        lblAuthType.Text = Strings.Label_AuthType;
        lblUsername.Text = Strings.Label_Username;
        lblPassword.Text = Strings.Label_Password;
        btnSave.Text = Strings.Btn_SaveAndConnect;
        btnTest.Text = Strings.Btn_TestConnection;
        btnCancel.Text = Strings.Btn_Cancel;

        cmbAuthType.Items.Clear();
        cmbAuthType.Items.Add(Strings.Auth_Windows);
        cmbAuthType.Items.Add(Strings.Auth_SqlServer);
        cmbAuthType.SelectedIndex = 0;
    }

    private void PopulateFromExistingSettings()
    {
        var connStr = _connectionStringProvider?.CurrentConnectionString
            ?? _settingsStore?.GetConnectionString()
            ?? "Server=(localdb)\\mssqllocaldb;Database=ParkGate_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        ParseConnectionString(connStr);
    }

    private void ParseConnectionString(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var isSqlAuth = false;

        foreach (var part in parts)
        {
            var kvp = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (kvp.Length != 2)
            {
                continue;
            }

            var key = kvp[0].ToLowerInvariant();
            var val = kvp[1];

            switch (key)
            {
                case "server":
                case "data source":
                case "addr":
                case "address":
                    txtServer.Text = val;
                    break;
                case "database":
                case "initial catalog":
                    txtDatabase.Text = val;
                    break;
                case "user id":
                case "uid":
                case "user":
                    txtUsername.Text = val;
                    isSqlAuth = true;
                    break;
                case "password":
                case "pwd":
                    txtPassword.Text = val;
                    isSqlAuth = true;
                    break;
                case "trusted_connection":
                case "integrated security":
                    if (val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("sspi", StringComparison.OrdinalIgnoreCase))
                    {
                        isSqlAuth = false;
                    }
                    break;
            }
        }

        cmbAuthType.SelectedIndex = isSqlAuth ? 1 : 0;
        txtUsername.Enabled = isSqlAuth;
        txtPassword.Enabled = isSqlAuth;
    }

    private void WireEvents()
    {
        cmbAuthType.SelectedIndexChanged += (_, _) =>
        {
            var isSqlAuth = cmbAuthType.SelectedIndex == 1;
            txtUsername.Enabled = isSqlAuth;
            txtPassword.Enabled = isSqlAuth;
            if (!isSqlAuth)
            {
                txtUsername.Clear();
                txtPassword.Clear();
            }
        };

        btnTest.Click += async (_, _) => await OnTestConnectionAsync();
        btnSave.Click += async (_, _) => await OnSaveConnectionAsync();
    }

    private string BuildConnectionString()
    {
        var server = txtServer.Text.Trim();
        var database = txtDatabase.Text.Trim();
        var isSqlAuth = cmbAuthType.SelectedIndex == 1;

        if (isSqlAuth)
        {
            var user = txtUsername.Text.Trim();
            var password = txtPassword.Text;
            return $"Server={server};Database={database};User Id={user};Password={password};MultipleActiveResultSets=true;TrustServerCertificate=True";
        }
        else
        {
            return $"Server={server};Database={database};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        }
    }

    private async Task OnTestConnectionAsync()
    {
        if (!ValidateInputs())
        {
            return;
        }

        SetBusy(true);
        lblStatus.ForeColor = Color.FromArgb(70, 70, 70);
        lblStatus.Text = "جاري اختبار الاتصال بقاعدة البيانات...";

        try
        {
            var connStr = BuildConnectionString();
            var canConnect = _startupService != null && await _startupService.CanConnectAsync(connStr);

            if (canConnect)
            {
                lblStatus.ForeColor = Color.FromArgb(27, 135, 63);
                lblStatus.Text = Strings.Msg_ConnectionSuccess;
                Notify.Info(Strings.Msg_ConnectionSuccess, Strings.Title_Info, this);
            }
            else
            {
                lblStatus.ForeColor = Color.FromArgb(198, 40, 40);
                lblStatus.Text = Strings.Msg_ConnectionFailed;
                Notify.Error(Strings.Msg_ConnectionFailed, Strings.Title_Error, this);
            }
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Color.FromArgb(198, 40, 40);
            lblStatus.Text = Strings.Msg_ConnectionFailed;
            Notify.Error(ex.Message, Strings.Title_Error, this);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task OnSaveConnectionAsync()
    {
        if (!ValidateInputs())
        {
            return;
        }

        SetBusy(true);
        try
        {
            var connStr = BuildConnectionString();

            // Check connection before saving
            if (_startupService != null)
            {
                var canConnect = await _startupService.CanConnectAsync(connStr);
                if (!canConnect)
                {
                    var proceedAnyway = Notify.Confirm(
                        Strings.Msg_ConnectionFailed + Environment.NewLine + "هل ترغب في حفظ هذه الإعدادات على أي حال؟",
                        Strings.Title_Confirm,
                        this);

                    if (!proceedAnyway)
                    {
                        return;
                    }
                }
            }

            _settingsStore?.SaveConnectionString(connStr);
            _connectionStringProvider?.UpdateConnectionString(connStr);

            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(txtServer.Text))
        {
            Notify.Warning(Strings.Msg_ServerRequired, Strings.Title_Warning, this);
            txtServer.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtDatabase.Text))
        {
            Notify.Warning(Strings.Msg_DatabaseRequired, Strings.Title_Warning, this);
            txtDatabase.Focus();
            return false;
        }

        if (cmbAuthType.SelectedIndex == 1 && string.IsNullOrWhiteSpace(txtUsername.Text))
        {
            Notify.Warning(Strings.Msg_SqlUserRequired, Strings.Title_Warning, this);
            txtUsername.Focus();
            return false;
        }

        return true;
    }

    private void SetBusy(bool busy)
    {
        btnSave.Enabled = !busy;
        btnTest.Enabled = !busy;
        btnCancel.Enabled = !busy;
        txtServer.Enabled = !busy;
        txtDatabase.Enabled = !busy;
        cmbAuthType.Enabled = !busy;
        if (!busy && cmbAuthType.SelectedIndex == 1)
        {
            txtUsername.Enabled = true;
            txtPassword.Enabled = true;
        }
        else if (busy)
        {
            txtUsername.Enabled = false;
            txtPassword.Enabled = false;
        }
    }
}
