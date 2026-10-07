using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using ParkGate.Application.AgeCategories.DTOs;
using ParkGate.Application.AgeCategories.Services;
using ParkGate.WinForms.Common;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;

namespace ParkGate.WinForms.Forms;

/// <summary>
/// Management form for age categories (FR-ADM-02).
/// Admin only.
/// </summary>
public partial class AgeCategoriesForm : RtlFormBase
{
    private readonly IAgeCategoryService? _ageCategoryService;
    private readonly IFormFactory? _formFactory;
    private readonly INotifyService? _notifyService;
    private readonly ILogger<AgeCategoriesForm>? _logger;

    private readonly List<AgeCategoryDto> _categories = [];
    private AgeCategoryDto? _selectedCategory;

    public AgeCategoriesForm() : this(null, null, null, null)
    {
    }

    public AgeCategoriesForm(
        IAgeCategoryService? ageCategoryService,
        IFormFactory? formFactory,
        INotifyService? notifyService,
        ILogger<AgeCategoriesForm>? logger)
    {
        InitializeComponent();

        _ageCategoryService = ageCategoryService;
        _formFactory = formFactory;
        _notifyService = notifyService;
        _logger = logger;

        InitializeFormText();
        ApplyGridOptimizations();
        WireEvents();
    }

    private void ApplyGridOptimizations()
    {
        // Reduce grid flicker per winforms-ui-design skill §6
        typeof(DataGridView)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(_gridAgeCategories, true, null);
    }

    private void InitializeFormText()
    {
        Text = Strings.Title_AgeCategories;
        _lblHeader.Text = Strings.Title_AgeCategories;
        _btnAdd.Text = Strings.Btn_Add;
        _btnEdit.Text = Strings.Btn_Edit;
        _btnToggleActive.Text = Strings.Btn_Deactivate;
        _btnRefresh.Text = Strings.Btn_Refresh;
        _btnClose.Text = Strings.Btn_Close;

        _colName.HeaderText = Strings.Col_Age_Name;
        _colDescription.HeaderText = Strings.Col_Age_Description;
        _colSortOrder.HeaderText = Strings.Col_Age_SortOrder;
        _colStatus.HeaderText = Strings.Col_Age_Status;
    }

    private void WireEvents()
    {
        Load += async (_, _) => await LoadAgeCategoriesAsync();
        _btnAdd.Click += async (_, _) => await AddAgeCategoryAsync();
        _btnEdit.Click += async (_, _) => await EditAgeCategoryAsync();
        _btnToggleActive.Click += async (_, _) => await ToggleActiveAsync();
        _btnRefresh.Click += async (_, _) => await LoadAgeCategoriesAsync();
        _btnClose.Click += (_, _) => Close();

        _gridAgeCategories.SelectionChanged += (_, _) => OnSelectionChanged();
        _gridAgeCategories.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0)
            {
                await EditAgeCategoryAsync();
            }
        };

        KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.F2)
            {
                await AddAgeCategoryAsync();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                await LoadAgeCategoriesAsync();
                e.Handled = true;
            }
        };
    }

    public async Task LoadAgeCategoriesAsync()
    {
        if (_ageCategoryService == null)
        {
            return;
        }

        try
        {
            var list = await _ageCategoryService.GetAllAsync();

            _categories.Clear();
            _categories.AddRange(list);

            PopulateGrid(_categories);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error loading age categories.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }

    private void PopulateGrid(IReadOnlyList<AgeCategoryDto> list)
    {
        _gridAgeCategories.SuspendLayout();
        _gridAgeCategories.Rows.Clear();

        foreach (var c in list)
        {
            var statusText = c.IsActive ? Strings.Status_Active : Strings.Status_Inactive;

            var rowIndex = _gridAgeCategories.Rows.Add(
                c.NameAr,
                c.DescriptionAr ?? "-",
                c.SortOrder,
                statusText);

            var row = _gridAgeCategories.Rows[rowIndex];
            row.Tag = c;

            if (c.IsActive)
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorGranted;
            }
            else
            {
                row.Cells[_colStatus.Index].Style.ForeColor = StatusBanner.ColorDenied;
            }
        }

        _gridAgeCategories.ResumeLayout(true);
        var activeCount = list.Count(c => c.IsActive);
        var inactiveCount = list.Count - activeCount;
        _lblCount.Text = $"إجمالي الفئات: {list.Count}  (النشطة: {activeCount} | المعطلة: {inactiveCount})";

        if (_gridAgeCategories.Rows.Count > 0)
        {
            _gridAgeCategories.Rows[0].Selected = true;
        }
        OnSelectionChanged();
    }

    private void OnSelectionChanged()
    {
        if (_gridAgeCategories.SelectedRows.Count > 0)
        {
            _selectedCategory = _gridAgeCategories.SelectedRows[0].Tag as AgeCategoryDto;
        }
        else if (_gridAgeCategories.CurrentRow != null)
        {
            _selectedCategory = _gridAgeCategories.CurrentRow.Tag as AgeCategoryDto;
        }
        else if (_gridAgeCategories.Rows.Count > 0 && _gridAgeCategories.Rows[0].Selected)
        {
            _selectedCategory = _gridAgeCategories.Rows[0].Tag as AgeCategoryDto;
        }
        else
        {
            _selectedCategory = null;
        }

        _btnEdit.Enabled = _selectedCategory != null;
        _btnToggleActive.Enabled = _selectedCategory != null;

        if (_selectedCategory != null)
        {
            _btnToggleActive.Text = _selectedCategory.IsActive
                ? Strings.Btn_Deactivate
                : Strings.Btn_Reactivate;
        }
    }

    public async Task AddAgeCategoryAsync()
    {
        var dialog = _formFactory?.Create<EditAgeCategoryDialog>() ??
                     new EditAgeCategoryDialog(_ageCategoryService, _notifyService, null);

        using (dialog)
        {
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadAgeCategoriesAsync();
            }
        }
    }

    public async Task EditAgeCategoryAsync()
    {
        if (_selectedCategory == null)
        {
            return;
        }

        var dialog = _formFactory?.Create<EditAgeCategoryDialog>() ??
                     new EditAgeCategoryDialog(_ageCategoryService, _notifyService, null);

        using (dialog)
        {
            dialog.SetAgeCategory(_selectedCategory);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await LoadAgeCategoriesAsync();
            }
        }
    }

    public async Task ToggleActiveAsync()
    {
        if (_selectedCategory == null || _ageCategoryService == null)
        {
            return;
        }

        var cat = _selectedCategory;
        var confirmMsg = cat.IsActive
            ? Strings.Msg_ConfirmDeactivateAge
            : Strings.Msg_ConfirmReactivateAge;

        var confirmed = _notifyService?.Confirm(confirmMsg, Strings.Title_Confirm) ??
                        (MessageBox.Show(
                            confirmMsg,
                            Strings.Title_Confirm,
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2,
                            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign) == DialogResult.Yes);

        if (!confirmed)
        {
            return;
        }

        try
        {
            var result = cat.IsActive
                ? await _ageCategoryService.DeactivateAsync(cat.Id)
                : await _ageCategoryService.ReactivateAsync(cat.Id);

            if (result.IsSuccess)
            {
                await LoadAgeCategoriesAsync();
            }
            else
            {
                _notifyService?.Warning(ErrorMessages.GetMessage(result.Error));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error toggling age category active state.");
            _notifyService?.Error(Strings.Msg_UnexpectedError);
        }
    }
}
