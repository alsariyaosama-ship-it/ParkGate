using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.CheckIn.DTOs;
using ParkGate.Application.CheckIn.Services;
using ParkGate.Application.Common;
using ParkGate.Application.Organizations.DTOs;
using ParkGate.Application.Organizations.Services;
using ParkGate.Application.Security;
using ParkGate.Domain.Enums;
using ParkGate.WinForms.Controls;
using ParkGate.WinForms.Forms;
using ParkGate.WinForms.Resources;
using ParkGate.WinForms.Services;
using Shouldly;

namespace ParkGate.ArchitectureTests;

public sealed class BeneficiariesManagementTests
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
    public void BeneficiariesControl_ParameterlessCtor_ShouldInitializeProperlyAndRtl()
    {
        using var control = new BeneficiariesControl();

        control.RightToLeft.ShouldBe(RightToLeft.Yes);

        var txtSearch = GetField<TextBox>(control, "_txtSearch");
        var btnSearch = GetField<Button>(control, "_btnSearch");
        var btnClear = GetField<Button>(control, "_btnClearSearch");
        var gridBen = GetField<DataGridView>(control, "_gridBeneficiaries");
        var grpDetails = GetField<GroupBox>(control, "_grpHolderDetails");
        var gridHist = GetField<DataGridView>(control, "_gridHistory");
        var btnEdit = GetField<Button>(control, "_btnEdit");
        var btnChangeStatus = GetField<Button>(control, "_btnChangeStatus");
        var btnCheckIn = GetField<Button>(control, "_btnCheckInDirect");
        var btnVoidHist = GetField<Button>(control, "_btnVoidHistoryEntry");

        txtSearch.Text.ShouldBeEmpty();
        btnSearch.Text.ShouldBe(Strings.Btn_Search);
        btnClear.Text.ShouldBe(Strings.Btn_ClearSearch);
        btnEdit.Text.ShouldBe(Strings.Btn_EditBeneficiary);
        btnChangeStatus.Text.ShouldBe(Strings.Btn_ChangeStatus);
        btnCheckIn.Text.ShouldBe(Strings.Btn_CheckInDirect);

        gridBen.ReadOnly.ShouldBeTrue();
        gridHist.ReadOnly.ShouldBeTrue();
        btnEdit.Enabled.ShouldBeFalse();
        btnChangeStatus.Enabled.ShouldBeFalse();
        btnCheckIn.Enabled.ShouldBeFalse();
        btnVoidHist.Enabled.ShouldBeFalse();
    }

    [Fact]
    public async Task BeneficiariesControl_SearchAndSelect_ShouldPopulateDetailsAndHistory()
    {
        var testHolderId = Guid.NewGuid();
        var fakeHolder = new BeneficiaryDto(
            Id: testHolderId,
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد عبد الله سالم",
            Phone: "0912345678",
            NationalId: "119850123456",
            CardExpiryDate: new DateOnly(2027, 12, 31),
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: DateTime.UtcNow.AddDays(-1),
            Notes: "مستفيد مسجل للتجربة",
            CreatedAtUtc: DateTime.UtcNow.AddMonths(-1));

        var fakeHistory = new List<EntryAttemptDto>
        {
            new(
                Id: Guid.NewGuid(),
                AttemptedAtUtc: DateTime.UtcNow.AddDays(-1),
                BusinessDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                WorkstationId: 1,
                UserId: 1,
                BeneficiaryId: testHolderId,
                Result: EntryResult.Allowed,
                DenialReason: null,
                OverrideReason: null,
                OverrideByUserId: null,
                TotalPersons: 3,
                IsVoided: false,
                VoidReason: null,
                VoidedAtUtc: null,
                VoidedByUserId: null,
                Companions: [new CompanionCountDto(1, "بالغ", 2)])
        };

        var fakeBenService = new FakeBeneficiaryService([fakeHolder], fakeHistory);
        var currentUser = new FakeCurrentUser(1, "supervisor1", "Park Supervisor", UserRole.Supervisor);

        using var control = new BeneficiariesControl(
            fakeBenService,
            null,
            null,
            currentUser,
            null,
            null,
            null,
            null);

        var txtSearch = GetField<TextBox>(control, "_txtSearch");
        var gridBen = GetField<DataGridView>(control, "_gridBeneficiaries");
        var gridHist = GetField<DataGridView>(control, "_gridHistory");
        var lblName = GetField<Label>(control, "_lblDetailName");
        var lblCard = GetField<Label>(control, "_lblDetailCard");
        var lblNid = GetField<Label>(control, "_lblDetailNid");
        var btnEdit = GetField<Button>(control, "_btnEdit");
        var btnChangeStatus = GetField<Button>(control, "_btnChangeStatus");

        txtSearch.Text = "100200";
        await control.PerformSearchAsync();

        gridBen.Rows.Count.ShouldBe(1);
        gridBen.Rows[0].Cells[0].Value.ShouldBe("100200");
        gridBen.Rows[0].Cells[2].Value.ShouldBe("محمد عبد الله سالم");

        // Supervisor sees unmasked national ID
        gridBen.Rows[0].Cells[4].Value.ShouldBe("119850123456");
        lblNid.Text.ShouldBe("119850123456");

        lblName.Text.ShouldBe("محمد عبد الله سالم");
        lblCard.Text.ShouldBe("100200");
        btnEdit.Enabled.ShouldBeTrue();
        btnChangeStatus.Enabled.ShouldBeTrue();

        gridHist.Rows.Count.ShouldBe(1);
        gridHist.Rows[0].Cells[3].Value.ShouldBe(Strings.Result_Allowed);
        gridHist.Rows[0].Cells[4].Value.ShouldBe(3);
    }

    [Fact]
    public async Task BeneficiariesControl_WhenOperatorUser_ShouldMaskNationalId()
    {
        var testHolderId = Guid.NewGuid();
        var fakeHolder = new BeneficiaryDto(
            Id: testHolderId,
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "100200",
            FullName: "محمد عبد الله سالم",
            Phone: "0912345678",
            NationalId: "119850123456",
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var fakeBenService = new FakeBeneficiaryService([fakeHolder], []);
        var currentUser = new FakeCurrentUser(1, "operator1", "Park Operator", UserRole.Operator);

        using var control = new BeneficiariesControl(
            fakeBenService,
            null,
            null,
            currentUser,
            null,
            null,
            null,
            null);

        var txtSearch = GetField<TextBox>(control, "_txtSearch");
        var gridBen = GetField<DataGridView>(control, "_gridBeneficiaries");
        var lblNid = GetField<Label>(control, "_lblDetailNid");
        var btnEdit = GetField<Button>(control, "_btnEdit");
        var btnChangeStatus = GetField<Button>(control, "_btnChangeStatus");

        txtSearch.Text = "100200";
        await control.PerformSearchAsync();

        gridBen.Rows.Count.ShouldBe(1);
        // Masked for operator: "********3456"
        gridBen.Rows[0].Cells[4].Value.ShouldBe("********3456");
        lblNid.Text.ShouldBe("********3456");

        // Operator does not have Edit/ChangeStatus permissions
        btnEdit.Visible.ShouldBeFalse();
        btnChangeStatus.Visible.ShouldBeFalse();
    }

    [Fact]
    public async Task BeneficiariesControl_CheckInDirect_ShouldFireRequestCheckInEvent()
    {
        var fakeHolder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "CARD-777",
            FullName: "سالم أحمد",
            Phone: "0912345678",
            NationalId: null,
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var fakeBenService = new FakeBeneficiaryService([fakeHolder], []);
        var currentUser = new FakeCurrentUser(1, "supervisor1", "Park Supervisor", UserRole.Supervisor);

        using var control = new BeneficiariesControl(
            fakeBenService,
            null,
            null,
            currentUser,
            null,
            null,
            null,
            null);

        string? requestedCard = null;
        control.RequestCheckIn += (_, card) => requestedCard = card;

        var btnCheckIn = GetField<Button>(control, "_btnCheckInDirect");
        control.SetSearchQuery("CARD-777");
        await control.PerformSearchAsync();

        btnCheckIn.PerformClick();

        requestedCard.ShouldBe("CARD-777");
    }

    [Fact]
    public void EditBeneficiaryDialog_ParameterlessCtor_ShouldInitializeProperly()
    {
        using var dialog = new EditBeneficiaryDialog();
        dialog.RightToLeft.ShouldBe(RightToLeft.Yes);
        dialog.Text.ShouldBe(Strings.Title_EditBeneficiary);
    }

    [Fact]
    public async Task EditBeneficiaryDialog_SetAndSave_ShouldCallUpdateAsync()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "CARD-888",
            FullName: "طارق علي",
            Phone: "0911112233",
            NationalId: "119900112233",
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: "ملاحظة أصلية",
            CreatedAtUtc: DateTime.UtcNow);

        var fakeBenService = new FakeBeneficiaryService([holder], []);
        var fakeOrgService = new FakeOrganizationService([new OrganizationDto(1, "TADAMUN", "صندوق التضامن", 720, true, true)]);

        var fakeNotifyService = new FakeNotifyService();
        using var dialog = new EditBeneficiaryDialog(fakeBenService, fakeOrgService, fakeNotifyService, null);
        await dialog.SetBeneficiaryAsync(holder);

        var txtFullName = GetField<TextBox>(dialog, "_txtFullName");
        var txtPhone = GetField<TextBox>(dialog, "_txtPhone");
        var txtNotes = GetField<TextBox>(dialog, "_txtNotes");

        txtFullName.Text.ShouldBe("طارق علي");
        txtPhone.Text.ShouldBe("0911112233");

        txtFullName.Text = "طارق علي محمود";
        txtNotes.Text = "تحديث الملاحظات";

        await dialog.SaveAsync();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.UpdatedBeneficiary.ShouldNotBeNull();
        dialog.UpdatedBeneficiary!.FullName.ShouldBe("طارق علي محمود");
        dialog.UpdatedBeneficiary.Notes.ShouldBe("تحديث الملاحظات");
    }

    [Fact]
    public void ChangeBeneficiaryStatusDialog_ParameterlessCtor_ShouldInitializeProperly()
    {
        using var dialog = new ChangeBeneficiaryStatusDialog();
        dialog.RightToLeft.ShouldBe(RightToLeft.Yes);
        dialog.Text.ShouldBe(Strings.Title_ChangeStatus);
    }

    [Fact]
    public async Task ChangeBeneficiaryStatusDialog_WhenSuspendedWithoutReason_ShouldNotProceed()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "CARD-999",
            FullName: "محمود حسن",
            Phone: "0912223344",
            NationalId: null,
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var fakeBenService = new FakeBeneficiaryService([holder], []);
        var fakeNotifyService = new FakeNotifyService();
        using var dialog = new ChangeBeneficiaryStatusDialog(fakeBenService, fakeNotifyService, null);
        dialog.SetBeneficiary(holder);

        var radSuspended = GetField<RadioButton>(dialog, "_radSuspended");
        var txtReason = GetField<TextBox>(dialog, "_txtReason");

        radSuspended.Checked = true;
        txtReason.Text = "123"; // Less than 5 characters

        await dialog.ConfirmChangeAsync();

        dialog.DialogResult.ShouldBe(DialogResult.None);
        dialog.UpdatedBeneficiary.ShouldBeNull();
        fakeNotifyService.Warnings.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ChangeBeneficiaryStatusDialog_WhenValidReason_ShouldCallChangeStatusAsync()
    {
        var holder = new BeneficiaryDto(
            Id: Guid.NewGuid(),
            OrganizationId: 1,
            OrganizationName: "صندوق التضامن",
            CardNumber: "CARD-999",
            FullName: "محمود حسن",
            Phone: "0912223344",
            NationalId: null,
            CardExpiryDate: null,
            Status: BeneficiaryStatus.Active,
            StatusReason: null,
            LastGrantedEntryUtc: null,
            Notes: null,
            CreatedAtUtc: DateTime.UtcNow);

        var fakeBenService = new FakeBeneficiaryService([holder], []);
        var fakeNotifyService = new FakeNotifyService();
        using var dialog = new ChangeBeneficiaryStatusDialog(fakeBenService, fakeNotifyService, null);
        dialog.SetBeneficiary(holder);

        var radSuspended = GetField<RadioButton>(dialog, "_radSuspended");
        var txtReason = GetField<TextBox>(dialog, "_txtReason");

        radSuspended.Checked = true;
        txtReason.Text = "إيقاف مؤقت لمراجعة البيانات";

        await dialog.ConfirmChangeAsync();

        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.UpdatedBeneficiary.ShouldNotBeNull();
        dialog.UpdatedBeneficiary!.Status.ShouldBe(BeneficiaryStatus.Suspended);
        dialog.UpdatedBeneficiary.StatusReason.ShouldBe("إيقاف مؤقت لمراجعة البيانات");
    }

    [Fact]
    public void VoidReasonDialog_Validation_RequiresMin5Chars()
    {
        var fakeNotifyService = new FakeNotifyService();
        using var dialog = new VoidReasonDialog(fakeNotifyService);
        dialog.RightToLeft.ShouldBe(RightToLeft.Yes);

        dialog.SetContext("CARD-1", "علي", "10:30 ص", 2);

        var txtReason = GetField<TextBox>(dialog, "_txtReason");
        var btnConfirm = GetField<Button>(dialog, "_btnConfirm");

        txtReason.Text = "خطأ";
        dialog.Confirm();
        dialog.DialogResult.ShouldBe(DialogResult.None);
        fakeNotifyService.Warnings.Count.ShouldBe(1);

        txtReason.Text = "تسجيل بطاقة خاطئة بالخطأ";
        dialog.Confirm();
        dialog.DialogResult.ShouldBe(DialogResult.OK);
        dialog.Reason.ShouldBe("تسجيل بطاقة خاطئة بالخطأ");
    }

    // Fakes for testing
    private sealed class FakeBeneficiaryService : IBeneficiaryService
    {
        private readonly List<BeneficiaryDto> _beneficiaries;
        private readonly List<EntryAttemptDto> _history;

        public FakeBeneficiaryService(List<BeneficiaryDto> beneficiaries, List<EntryAttemptDto> history)
        {
            _beneficiaries = beneficiaries;
            _history = history;
        }

        public Task<Result<IReadOnlyList<BeneficiaryLookupResultDto>>> LookupByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Result<BeneficiaryLookupResultDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var match = _beneficiaries.FirstOrDefault(b => b.Id == id);
            if (match == null)
            {
                return Task.FromResult(Result<BeneficiaryLookupResultDto>.Failure(Errors.Beneficiary.NotFound));
            }

            var lookup = new BeneficiaryLookupResultDto(
                match,
                new EligibilityPreviewDto(true, EntryResult.Allowed, null, string.Empty, false, 720, match.LastGrantedEntryUtc, null, null),
                []);
            return Task.FromResult(Result<BeneficiaryLookupResultDto>.Success(lookup));
        }

        public Task<Result<BeneficiaryDto>> RegisterAsync(RegisterBeneficiaryRequest request, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Result<BeneficiaryDto>> UpdateAsync(UpdateBeneficiaryRequest request, CancellationToken cancellationToken = default)
        {
            var existing = _beneficiaries.FirstOrDefault(b => b.Id == request.Id);
            if (existing == null)
            {
                return Task.FromResult(Result<BeneficiaryDto>.Failure(Errors.Beneficiary.NotFound));
            }

            var updated = existing with
            {
                OrganizationId = request.OrganizationId,
                CardNumber = request.CardNumber,
                FullName = request.FullName,
                Phone = request.Phone,
                NationalId = request.NationalId,
                CardExpiryDate = request.CardExpiryDate,
                Notes = request.Notes
            };

            var idx = _beneficiaries.IndexOf(existing);
            _beneficiaries[idx] = updated;

            return Task.FromResult(Result<BeneficiaryDto>.Success(updated));
        }

        public Task<Result<BeneficiaryDto>> ChangeStatusAsync(ChangeBeneficiaryStatusRequest request, CancellationToken cancellationToken = default)
        {
            var existing = _beneficiaries.FirstOrDefault(b => b.Id == request.Id);
            if (existing == null)
            {
                return Task.FromResult(Result<BeneficiaryDto>.Failure(Errors.Beneficiary.NotFound));
            }

            var updated = existing with
            {
                Status = request.TargetStatus,
                StatusReason = request.Reason
            };

            var idx = _beneficiaries.IndexOf(existing);
            _beneficiaries[idx] = updated;

            return Task.FromResult(Result<BeneficiaryDto>.Success(updated));
        }

        public Task<Result<IReadOnlyList<BeneficiaryDto>>> SearchAsync(string searchTerm, int maxResults = 50, CancellationToken cancellationToken = default)
        {
            var matches = _beneficiaries.Where(b =>
                b.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.CardNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.Phone.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (b.NationalId != null && b.NationalId.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))).ToList();

            return Task.FromResult(Result<IReadOnlyList<BeneficiaryDto>>.Success(matches));
        }

        public Task<Result<IReadOnlyList<EntryAttemptDto>>> GetHistoryAsync(Guid beneficiaryId, int maxResults = 100, CancellationToken cancellationToken = default)
        {
            var history = _history.Where(h => h.BeneficiaryId == beneficiaryId).ToList();
            return Task.FromResult(Result<IReadOnlyList<EntryAttemptDto>>.Success(history));
        }
    }

    private sealed class FakeOrganizationService : IOrganizationService
    {
        private readonly List<OrganizationDto> _orgs;

        public FakeOrganizationService(List<OrganizationDto> orgs)
        {
            _orgs = orgs;
        }

        public Task<IReadOnlyList<OrganizationDto>> GetAllAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<OrganizationDto>>(_orgs);

        public Task<Result<OrganizationDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<OrganizationDto>> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result<OrganizationDto>> UpdateAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Result> ReactivateAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public FakeCurrentUser(int userId, string userName, string fullName, UserRole role)
        {
            UserId = userId;
            UserName = userName;
            FullName = fullName;
            Role = role;
        }

        public int? UserId { get; }
        public string? UserName { get; }
        public string? FullName { get; }
        public UserRole? Role { get; }
        public bool IsAuthenticated => true;

        public bool Has(Permission permission) => RolePermissions.GetPermissions(Role ?? UserRole.Operator).Contains(permission);
        public bool HasAny(params Permission[] permissions) => permissions.Any(Has);
        public bool IsInRole(UserRole role) => Role == role;
    }

    private sealed class FakeNotifyService : INotifyService
    {
        public List<string> Infos { get; } = [];
        public List<string> Warnings { get; } = [];
        public List<string> Errors { get; } = [];
        public bool NextConfirmResult { get; set; } = true;

        public void Info(string message, string? title = null) => Infos.Add(message);
        public void Warning(string message, string? title = null) => Warnings.Add(message);
        public void Error(string message, string? title = null) => Errors.Add(message);
        public bool Confirm(string message, string? title = null) => NextConfirmResult;
    }
}
