using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.GiftVouchers.DTOs;
using ParkGate.Application.GiftVouchers.Services;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Ports.Reports;
using ParkGate.Application.Settings.Services;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

public partial class GiftReceiptsForm : RtlFormBase
{
    private readonly IGiftReportService? _giftReportService;
    private readonly IGiftExcelExportService? _giftExcelExportService;
    private readonly IOrganizationService? _organizationService;
    private readonly ISettingsService? _settingsService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<GiftReceiptsForm>? _logger;

    private List<OrganizationDto> _organizations = [];
    private GiftReceiptReportDto? _currentReport;
    private string _timeZoneId = "Libya Standard Time";
    private bool _isBusy;

    public GiftReceiptsForm() : this(null, null, null, null, null, null)
    {
    }

    public GiftReceiptsForm(
        IGiftReportService? giftReportService,
        IGiftExcelExportService? giftExcelExportService,
        IOrganizationService? organizationService,
        ISettingsService? settingsService,
        INotifyService? notifyService,
        ILogger<GiftReceiptsForm>? logger)
    {
        InitializeComponent();

        _giftReportService = giftReportService;
        _giftExcelExportService = giftExcelExportService;
        _organizationService = organizationService;
        _settingsService = settingsService;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        Theme.ApplyGridStyle(_gridReceipts);
        SetupColumns();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_GiftReceipts;
        _lblFromDate.Text = Strings.Label_DateFrom;
        _lblToDate.Text = Strings.Label_DateTo;
        _lblOrganization.Text = Strings.Col_Org;
        _lblStatus.Text = Strings.Label_FilterStatus;

        _btnRefresh.Text = Strings.Btn_Refresh;
        _btnExportExcel.Text = Strings.Btn_ExportExcel;
        _btnPrintPreview.Text = Strings.Btn_Print;

        Theme.ApplyPrimaryButtonStyle(_btnRefresh);
        Theme.ApplyPrimaryButtonStyle(_btnExportExcel);
        Theme.ApplySecondaryButtonStyle(_btnPrintPreview);
    }

    private void SetupColumns()
    {
        _gridReceipts.AutoGenerateColumns = false;
        _gridReceipts.Columns.Clear();

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GiftReceiptRowDto.ReceiptNo),
            HeaderText = Strings.Col_Gift_ReceiptNo,
            Width = 90
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GiftReceiptRowDto.HolderName),
            HeaderText = Strings.Col_Gift_HolderName,
            Width = 140
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GiftReceiptRowDto.CardNumber),
            HeaderText = Strings.Col_Gift_CardNumber,
            Width = 100
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GiftReceiptRowDto.OrganizationName),
            HeaderText = Strings.Col_Gift_Organization,
            Width = 120
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ValueCol",
            HeaderText = Strings.Col_Gift_Value,
            Width = 90
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "StatusCol",
            HeaderText = Strings.Col_Gift_Status,
            Width = 90
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IssuedAtCol",
            HeaderText = Strings.Col_Gift_IssuedAt,
            Width = 130
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GiftReceiptRowDto.IssuedByUserName),
            HeaderText = Strings.Col_Gift_IssuedBy,
            Width = 90
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "RedeemedAtCol",
            HeaderText = Strings.Col_Gift_RedeemedAt,
            Width = 130
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GiftReceiptRowDto.RedeemedByUserName),
            HeaderText = Strings.Col_Gift_RedeemedBy,
            Width = 90
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GiftReceiptRowDto.RedeemedWorkstationName),
            HeaderText = Strings.Col_Gift_RedeemedWs,
            Width = 100
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "OverrideCol",
            HeaderText = Strings.Col_Gift_IsOverride,
            Width = 70
        });

        _gridReceipts.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ReasonCol",
            HeaderText = Strings.Col_Gift_Reason,
            Width = 150
        });

        _gridReceipts.CellFormatting += OnGridCellFormatting;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await OnFormLoadAsync();
        _btnRefresh.Click += async (_, _) => await LoadReportAsync();
        _btnExportExcel.Click += async (_, _) => await ExportExcelAsync();
        _btnPrintPreview.Click += (_, _) => ShowPrintPreview();
    }

    private async Task OnFormLoadAsync()
    {
        try
        {
            if (_settingsService != null)
            {
                var settings = await _settingsService.GetSettingsAsync();
                _timeZoneId = settings.TimeZoneId;
            }

            // Populate status filter combo
            _cmbStatus.Items.Clear();
            _cmbStatus.Items.Add(new ComboBoxItem<GiftReceiptFilter>(GiftReceiptFilter.All, Strings.Status_All));
            _cmbStatus.Items.Add(new ComboBoxItem<GiftReceiptFilter>(GiftReceiptFilter.Redeemed, Strings.Status_RedeemedVoucher));
            _cmbStatus.Items.Add(new ComboBoxItem<GiftReceiptFilter>(GiftReceiptFilter.Issued, Strings.Status_IssuedVoucher));
            _cmbStatus.Items.Add(new ComboBoxItem<GiftReceiptFilter>(GiftReceiptFilter.Voided, Strings.Status_VoidedVoucher));
            _cmbStatus.SelectedIndex = 0;

            // Dates default: today
            _dtpFromDate.Value = DateTime.Today;
            _dtpToDate.Value = DateTime.Today;

            // Load organizations
            if (_organizationService != null)
            {
                _organizations = (await _organizationService.GetAllAsync()).ToList();
                _cmbOrganization.Items.Clear();
                _cmbOrganization.Items.Add(new ComboBoxItem<int?>(null, Strings.Filter_AllOrganizations));
                foreach (var org in _organizations)
                {
                    _cmbOrganization.Items.Add(new ComboBoxItem<int?>(org.Id, org.NameAr));
                }
                _cmbOrganization.SelectedIndex = 0;
            }

            await LoadReportAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize GiftReceiptsForm");
            _notifyService?.Error(UserMessages.Describe(ex));
        }
    }

    public async Task LoadReportAsync()
    {
        if (_isBusy || _giftReportService == null)
        {
            return;
        }

        SetBusy(true);
        try
        {
            var from = DateOnly.FromDateTime(_dtpFromDate.Value);
            var to = DateOnly.FromDateTime(_dtpToDate.Value);

            int? orgId = null;
            if (_cmbOrganization.SelectedItem is ComboBoxItem<int?> orgItem)
            {
                orgId = orgItem.Value;
            }

            var filter = GiftReceiptFilter.All;
            if (_cmbStatus.SelectedItem is ComboBoxItem<GiftReceiptFilter> filterItem)
            {
                filter = filterItem.Value;
            }

            var result = await _giftReportService.GetReceiptsAsync(from, to, orgId, null, filter);
            if (!result.IsSuccess)
            {
                _notifyService?.Error(ErrorMessages.GetMessage(result.Error.Code));
                return;
            }

            _currentReport = result.Value;
            _gridReceipts.DataSource = _currentReport.Rows.ToList();
            UpdateSummaryDisplay(_currentReport);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load gift receipts report");
            _notifyService?.Error(UserMessages.Describe(ex));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateSummaryDisplay(GiftReceiptReportDto report)
    {
        var redeemed = report.StatusTotals.FirstOrDefault(s => s.Status == GiftVoucherStatus.Redeemed);
        var issued = report.StatusTotals.FirstOrDefault(s => s.Status == GiftVoucherStatus.Issued);
        var voided = report.StatusTotals.FirstOrDefault(s => s.Status == GiftVoucherStatus.Voided);

        _lblSummary.Text = string.Format(
            Strings.Summary_GiftReceipts,
            report.TotalCount,
            report.TotalValue,
            redeemed?.Count ?? 0,
            redeemed?.TotalValue ?? 0m,
            issued?.Count ?? 0,
            issued?.TotalValue ?? 0m,
            voided?.Count ?? 0,
            voided?.TotalValue ?? 0m);
    }

    private void OnGridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _gridReceipts.Rows.Count)
        {
            return;
        }

        if (_gridReceipts.Rows[e.RowIndex].DataBoundItem is not GiftReceiptRowDto row)
        {
            return;
        }

        var col = _gridReceipts.Columns[e.ColumnIndex];
        if (col.Name == "ValueCol")
        {
            e.Value = row.Value.ToString("N3");
            e.FormattingApplied = true;
        }
        else if (col.Name == "StatusCol")
        {
            e.Value = row.Status switch
            {
                GiftVoucherStatus.Issued => Strings.Status_IssuedVoucher,
                GiftVoucherStatus.Redeemed => Strings.Status_RedeemedVoucher,
                GiftVoucherStatus.Voided => Strings.Status_VoidedVoucher,
                _ => row.Status.ToString()
            };
            e.FormattingApplied = true;
        }
        else if (col.Name == "IssuedAtCol")
        {
            e.Value = FormatDateTime(row.IssuedAtUtc);
            e.FormattingApplied = true;
        }
        else if (col.Name == "RedeemedAtCol")
        {
            e.Value = row.RedeemedAtUtc.HasValue ? FormatDateTime(row.RedeemedAtUtc.Value) : "-";
            e.FormattingApplied = true;
        }
        else if (col.Name == "OverrideCol")
        {
            e.Value = row.IsOverride ? Strings.Status_Active : "-";
            e.FormattingApplied = true;
        }
        else if (col.Name == "ReasonCol")
        {
            e.Value = row.Status == GiftVoucherStatus.Voided
                ? row.VoidReason ?? "-"
                : (row.IsOverride ? row.OverrideReason ?? "-" : "-");
            e.FormattingApplied = true;
        }
    }

    private string FormatDateTime(DateTime utcTime)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId);
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), tz);
            return local.ToString("dd/MM/yyyy hh:mm tt");
        }
        catch
        {
            return utcTime.ToString("dd/MM/yyyy HH:mm");
        }
    }

    public async Task ExportExcelAsync()
    {
        if (_isBusy || _currentReport == null || _currentReport.Rows.Count == 0 || _giftExcelExportService == null)
        {
            if (_currentReport == null || _currentReport.Rows.Count == 0)
            {
                _notifyService?.Info(Strings.Msg_NoGiftReceiptsFound);
            }
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "Excel Files (*.xlsx)|*.xlsx",
            FileName = $"GiftReceipts_{_currentReport.FromDate:yyyyMMdd}_{_currentReport.ToDate:yyyyMMdd}.xlsx",
            Title = Strings.Btn_ExportExcel
        };

        if (sfd.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        SetBusy(true);
        try
        {
            var bytes = _giftExcelExportService.ExportGiftReceipts(_currentReport, _timeZoneId);
            await File.WriteAllBytesAsync(sfd.FileName, bytes);
            _notifyService?.Info(Strings.Msg_ExportExcelSuccess);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to export gift receipts to Excel");
            _notifyService?.Error(UserMessages.Describe(ex));
        }
        finally
        {
            SetBusy(false);
        }
    }

    public void ShowPrintPreview()
    {
        if (_currentReport == null || _currentReport.Rows.Count == 0)
        {
            _notifyService?.Info(Strings.Msg_NoGiftReceiptsFound);
            return;
        }

        try
        {
            using var doc = new PrintDocument();
            doc.DefaultPageSettings.Landscape = true;
            doc.PrintPage += OnPrintPage;

            using var preview = new PrintPreviewDialog
            {
                Document = doc,
                Width = 900,
                Height = 650,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };
            preview.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to show print preview for gift receipts");
            _notifyService?.Error(UserMessages.Describe(ex));
        }
    }

    private void OnPrintPage(object sender, PrintPageEventArgs e)
    {
        var g = e.Graphics;
        if (g == null)
        {
            return;
        }

        using var fontTitle = new Font("Cairo", 14, FontStyle.Bold);
        using var fontHeader = new Font("Cairo", 9, FontStyle.Bold);
        using var fontData = new Font("Cairo", 8, FontStyle.Regular);
        using var fontSummary = new Font("Cairo", 9, FontStyle.Bold);

        float x = e.MarginBounds.Left;
        float y = e.MarginBounds.Top;
        float width = e.MarginBounds.Width;

        using var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var sfRight = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.DirectionRightToLeft };

        // Title
        g.DrawString(Strings.Title_GiftReceipts, fontTitle, Brushes.Black, new RectangleF(x, y, width, 30), sfCenter);
        y += 35;

        // Subtitle
        var subtitle = $"{Strings.Label_DateFrom} {_dtpFromDate.Value:dd/MM/yyyy}  {Strings.Label_DateTo} {_dtpToDate.Value:dd/MM/yyyy}";
        g.DrawString(subtitle, fontHeader, Brushes.Gray, new RectangleF(x, y, width, 20), sfCenter);
        y += 28;

        // Grid columns to print
        string[] printHeaders = {
            Strings.Col_Gift_ReceiptNo,
            Strings.Col_Gift_HolderName,
            Strings.Col_Gift_CardNumber,
            Strings.Col_Gift_Organization,
            Strings.Col_Gift_Value,
            Strings.Col_Gift_Status,
            Strings.Col_Gift_IssuedAt,
            Strings.Col_Gift_RedeemedAt
        };
        float colWidth = width / printHeaders.Length;

        // Header row
        for (int c = 0; c < printHeaders.Length; c++)
        {
            var rect = new RectangleF(x + (printHeaders.Length - 1 - c) * colWidth, y, colWidth, 24);
            g.FillRectangle(new SolidBrush(Color.FromArgb(27, 135, 63)), rect);
            g.DrawRectangle(Pens.DarkGreen, rect.X, rect.Y, rect.Width, rect.Height);
            g.DrawString(printHeaders[c], fontHeader, Brushes.White, rect, sfCenter);
        }
        y += 24;

        // Print up to 35 rows
        if (_currentReport != null)
        {
            int maxRows = Math.Min(_currentReport.Rows.Count, 30);
            for (int r = 0; r < maxRows; r++)
            {
                var row = _currentReport.Rows[r];
                string[] vals = {
                    row.ReceiptNo.ToString(),
                    row.HolderName,
                    row.CardNumber,
                    row.OrganizationName,
                    row.Value.ToString("N3"),
                    row.Status switch { GiftVoucherStatus.Issued => Strings.Status_IssuedVoucher, GiftVoucherStatus.Redeemed => Strings.Status_RedeemedVoucher, _ => Strings.Status_VoidedVoucher },
                    FormatDateTime(row.IssuedAtUtc),
                    row.RedeemedAtUtc.HasValue ? FormatDateTime(row.RedeemedAtUtc.Value) : "-"
                };

                for (int c = 0; c < vals.Length; c++)
                {
                    var rect = new RectangleF(x + (vals.Length - 1 - c) * colWidth, y, colWidth, 20);
                    if (r % 2 == 1)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(248, 250, 252)), rect);
                    }
                    g.DrawRectangle(Pens.LightGray, rect.X, rect.Y, rect.Width, rect.Height);
                    g.DrawString(vals[c], fontData, Brushes.Black, rect, sfCenter);
                }
                y += 20;
            }

            // Summary line
            y += 10;
            g.DrawString(_lblSummary.Text, fontSummary, new SolidBrush(Color.FromArgb(27, 135, 63)), new RectangleF(x, y, width, 25), sfRight);
        }

        e.HasMorePages = false;
    }

    private void SetBusy(bool busy)
    {
        _isBusy = busy;
        _btnRefresh.Enabled = !busy;
        _btnExportExcel.Enabled = !busy;
        _btnPrintPreview.Enabled = !busy;
        _dtpFromDate.Enabled = !busy;
        _dtpToDate.Enabled = !busy;
        _cmbOrganization.Enabled = !busy;
        _cmbStatus.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private sealed class ComboBoxItem<T>(T value, string text)
    {
        public T Value { get; } = value;
        public string Text { get; } = text;

        public override string ToString() => Text;
    }
}
