using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Modal dialog for adding or editing an age category (FR-ADM-02).
/// Admin only.
/// </summary>
public partial class EditAgeCategoryDialog : RtlFormBase
{
    private readonly IAgeCategoryService? _ageCategoryService;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<EditAgeCategoryDialog>? _logger;

    private AgeCategoryDto? _existingCategory;

    public AgeCategoryDto? SavedCategory { get; private set; }

    public EditAgeCategoryDialog() : this(null, null, null)
    {
    }

    public EditAgeCategoryDialog(
        IAgeCategoryService? ageCategoryService,
        INotifyService? notifyService,
        ILogger<EditAgeCategoryDialog>? logger)
    {
        InitializeComponent();

        _ageCategoryService = ageCategoryService;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        WireEvents();
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_AddAgeCategory;
        _lblHeader.Text = Strings.Title_AddAgeCategory;
        _lblNameAr.Text = Strings.Label_Age_Name;
        _lblDescriptionAr.Text = Strings.Label_Age_Description;
        _lblSortOrder.Text = Strings.Label_Age_SortOrder;
        _btnSave.Text = Strings.Btn_Save;
        _btnCancel.Text = Strings.Btn_Cancel;
    }

    private void WireEvents()
    {
        Shown += (_, _) => _txtNameAr.Focus();
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetAgeCategory(AgeCategoryDto category)
    {
        _existingCategory = category;

        Text = Strings.Title_EditAgeCategory;
        _lblHeader.Text = Strings.Title_EditAgeCategory;

        _txtNameAr.Text = category.NameAr;
        _txtDescriptionAr.Text = category.DescriptionAr ?? string.Empty;
        _numSortOrder.Value = Math.Max(0, Math.Min(100, category.SortOrder));
    }

    public async Task SaveAsync()
    {
        if (_ageCategoryService == null)
        {
            return;
        }

        var nameAr = _txtNameAr.Text.Trim();
        var descriptionAr = string.IsNullOrWhiteSpace(_txtDescriptionAr.Text)
            ? null
            : _txtDescriptionAr.Text.Trim();
        var sortOrder = (int)_numSortOrder.Value;

        if (string.IsNullOrWhiteSpace(nameAr))
        {
            ShowWarning("اسم الفئة العمرية مطلوب.");
            _txtNameAr.Focus();
            return;
        }

        using (new BusyScope(this, _btnSave, _btnCancel))
        {
            try
            {
                if (_existingCategory == null)
                {
                    var request = new CreateAgeCategoryRequest(nameAr, descriptionAr, sortOrder);
                    var result = await _ageCategoryService.CreateAsync(request);
                    if (result.IsSuccess)
                    {
                        SavedCategory = result.Value;
                        _notifyService?.Info(Strings.Msg_SaveAgeSuccess);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        ShowWarning(ErrorMessages.GetMessage(result.Error));
                    }
                }
                else
                {
                    var request = new UpdateAgeCategoryRequest(_existingCategory.Id, nameAr, descriptionAr, sortOrder);
                    var result = await _ageCategoryService.UpdateAsync(request);
                    if (result.IsSuccess)
                    {
                        SavedCategory = result.Value;
                        _notifyService?.Info(Strings.Msg_SaveAgeSuccess);
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                    else
                    {
                        ShowWarning(ErrorMessages.GetMessage(result.Error));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving age category.");
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
