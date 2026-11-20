using ParkGate.Application.Audit;
using ParkGate.Application.Common;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;

namespace ParkGate.Application.Tests.Audit;

/// <summary>R5.20: the audit trail can be searched and read on screen.</summary>
public sealed class AuditLogServiceTests
{
    private readonly CurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWorkFactory _uowFactory;
    private readonly AuditLogService _sut;
    private readonly User _operator;
    private readonly User _supervisor;

    public AuditLogServiceTests()
    {
        _operator = new User("op1", "موظف الشباك", "hash", UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(_operator, 1);
        _supervisor = new User("sup1", "المشرف سالم", "hash", UserRole.Supervisor);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(_supervisor, 2);
        var desk = new Workstation("DESK-01", "الشباك 1", WorkstationType.Desk);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(desk, 1);

        var settings = new FakeAppSettingRepository();
        settings.Add(new AppSetting("Park.TimeZoneId", "Libya Standard Time", DateTime.UtcNow, null));
        _uowFactory = new FakeUnitOfWorkFactory(users: [_operator, _supervisor], workstations: [desk], appSettings: settings);
        _sut = new AuditLogService(_uowFactory, _currentUser, TestSettings.Over(settings));
        _currentUser.SetUser(_supervisor);
    }

    [Fact]
    public async Task SearchAsync_AsOperator_IsForbidden()
    {
        _currentUser.SetUser(_operator);

        var result = await _sut.SearchAsync(Request(AuditCategory.All));

        result.Error.Code.ShouldBe(Errors.General.Forbidden.Code);
    }

    [Fact]
    public async Task SearchAsync_GiftVoided_ShowsVisitorReasonOperatorAndApprover()
    {
        Log(new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc), "Gift.Voided", 2,
            """{"BeneficiaryId":"x","CardNumber":"100200","HolderName":"محمد أحمد","OrganizationId":1,"BusinessDate":"2026-11-20","OperatorUserId":1,"ApprovedByUserId":2,"ReceiptNo":42,"Reason":"طباعة خطأ"}""");

        var row = (await _sut.SearchAsync(Request(AuditCategory.All))).Value.Rows.Single();

        row.Action.ShouldBe("Gift.Voided");
        row.ActorName.ShouldBe("المشرف سالم");
        row.WorkstationName.ShouldBe("الشباك 1");
        row.CardNumber.ShouldBe("100200");
        row.HolderName.ShouldBe("محمد أحمد");
        row.Reason.ShouldBe("طباعة خطأ");
        row.OperatorName.ShouldBe("موظف الشباك");
        row.ApprovedByName.ShouldBe("المشرف سالم");
        row.DetailsJson.ShouldNotBeNull();
        row.DetailsJson.ShouldContain("ReceiptNo");
    }

    [Fact]
    public async Task SearchAsync_ByCategory_KeepsOnlyThatKindNewestFirst()
    {
        Log(new DateTime(2026, 11, 20, 8, 0, 0, DateTimeKind.Utc), "EntryGranted", 1, """{"CardNumber":"1"}""");
        Log(new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc), "Gift.Issued", 1, """{"CardNumber":"1"}""");
        Log(new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc), "Gift.Redeemed", 1, """{"CardNumber":"1"}""");
        Log(new DateTime(2026, 11, 20, 11, 0, 0, DateTimeKind.Utc), "UserLogin", 1, null);

        var gifts = (await _sut.SearchAsync(Request(AuditCategory.Gifts))).Value.Rows;
        var entries = (await _sut.SearchAsync(Request(AuditCategory.Entries))).Value.Rows;
        var users = (await _sut.SearchAsync(Request(AuditCategory.Users))).Value.Rows;

        gifts.Select(r => r.Action).ShouldBe(["Gift.Redeemed", "Gift.Issued"]);
        entries.Select(r => r.Action).ShouldBe(["EntryGranted"]);
        users.Select(r => r.Action).ShouldBe(["UserLogin"]);
    }

    [Fact]
    public async Task SearchAsync_ByCardNumber_FindsOnlyThatVisitor()
    {
        Log(new DateTime(2026, 11, 20, 8, 0, 0, DateTimeKind.Utc), "EntryGranted", 1, """{"CardNumber":"100200"}""");
        Log(new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc), "EntryGranted", 1, """{"CardNumber":"1002001"}""");

        var rows = (await _sut.SearchAsync(Request(AuditCategory.All) with { CardNumber = " ١٠٠٢٠٠ " })).Value.Rows;

        rows.Single().CardNumber.ShouldBe("100200");
    }

    [Fact]
    public async Task SearchAsync_DatesAreParkLocalDays()
    {
        // Libya is UTC+2: 21:59 UTC on the 19th is the 19th, 22:00 UTC is already the 20th.
        Log(new DateTime(2026, 11, 19, 21, 59, 0, DateTimeKind.Utc), "EntryGranted", 1, null);
        Log(new DateTime(2026, 11, 19, 22, 0, 0, DateTimeKind.Utc), "EntryVoided", 1, null);

        var rows = (await _sut.SearchAsync(Request(AuditCategory.All))).Value.Rows;

        rows.Select(r => r.Action).ShouldBe(["EntryVoided"]);
    }

    [Fact]
    public async Task SearchAsync_MoreRowsThanTheLimit_SaysTheListIsCut()
    {
        for (var i = 0; i < AuditLogService.MaxRows + 1; i++)
        {
            Log(new DateTime(2026, 11, 20, 8, 0, 0, DateTimeKind.Utc).AddSeconds(i), "EntryGranted", 1, null);
        }

        var page = (await _sut.SearchAsync(Request(AuditCategory.All))).Value;

        page.Rows.Count.ShouldBe(AuditLogService.MaxRows);
        page.IsTruncated.ShouldBeTrue();
    }

    private static AuditLogSearchRequest Request(AuditCategory category) =>
        new(new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 20), category, null);

    private void Log(DateTime atUtc, string action, int userId, string? details) =>
        _uowFactory.CurrentUow.AuditLogs.Add(new AuditLog(atUtc, action, userId, 1, null, null, details));
}
