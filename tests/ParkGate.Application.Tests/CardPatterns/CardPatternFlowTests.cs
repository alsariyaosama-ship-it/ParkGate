using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Beneficiaries.Services;
using ParkGate.Application.Beneficiaries.Validators;
using ParkGate.Application.CardPatterns;
using ParkGate.Application.Common;
using ParkGate.Application.Security;
using ParkGate.Application.Tests.Common;
using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Tests.CardPatterns;

/// <summary>F8, BR-29: registering and changing card numbers learns the usual pattern and raises alerts; the Admin decides.</summary>
public sealed class CardPatternFlowTests
{
    private readonly List<Beneficiary> _beneficiaries = [];
    private readonly CurrentUserContext _currentUser = new();
    private readonly FakeUnitOfWorkFactory _uowFactory;
    private readonly BeneficiaryService _beneficiaries_;
    private readonly CardPatternService _patterns;
    private readonly Organization _org;
    private readonly User _operator;
    private readonly User _admin;
    private int _phone;

    public CardPatternFlowTests()
    {
        _org = new Organization("SSF", "صندوق التضامن", isDefault: true);
        typeof(Organization).GetProperty(nameof(Organization.Id))!.SetValue(_org, 1);
        _operator = new User("op1", "موظف الشباك", "hash", UserRole.Operator);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(_operator, 1);
        _admin = new User("admin", "المدير", "hash", UserRole.Admin);
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(_admin, 2);
        var desk = new Workstation("DESK-01", "الشباك 1", WorkstationType.Desk);
        typeof(Workstation).GetProperty(nameof(Workstation.Id))!.SetValue(desk, 1);
        var currentDesk = new CurrentWorkstationContext("DESK-01");
        currentDesk.SetWorkstation(desk);

        _uowFactory = new FakeUnitOfWorkFactory(
            users: [_operator, _admin], workstations: [desk], organizations: [_org], beneficiaries: _beneficiaries);
        _beneficiaries_ = new BeneficiaryService(
            _uowFactory, new RollingWindowEligibilityPolicy(), _currentUser, currentDesk,
            new RegisterBeneficiaryRequestValidator(), new UpdateBeneficiaryRequestValidator(), new ChangeBeneficiaryStatusRequestValidator());
        _patterns = new CardPatternService(_uowFactory, _currentUser, currentDesk);
        _currentUser.SetUser(_operator);
    }

    private List<OrganizationCardPattern> Patterns => _uowFactory.CurrentUow.CardPatternsFake.Patterns;

    private List<CardPatternAlert> Alerts => _uowFactory.CurrentUow.CardPatternAlertsFake.Alerts;

    [Fact]
    public async Task Register_NineCards_LearnsNothingAndRaisesNothing()
    {
        for (var i = 1; i <= 9; i++)
        {
            (await RegisterAsync($"10{i:D4}")).HasOpenCardAlert.ShouldBeFalse();
        }

        Patterns.ShouldBeEmpty();
        Alerts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Register_TenthCard_ProposesTheDominantPatternAndAuditsIt()
    {
        await RegisterManyAsync("10", 10);

        var pattern = Patterns.ShouldHaveSingleItem();
        pattern.Shape.ShouldBe("DDDDDD");
        pattern.Prefix.ShouldBe("10");
        pattern.Source.ShouldBe(CardPatternSource.Dominant);
        pattern.Status.ShouldBe(CardPatternStatus.Proposed);
        _uowFactory.CurrentUow.AuditLogRows.ShouldContain(a => a.Action == "CardPattern.Proposed");
        Alerts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Register_UnusualNumber_IsSavedWithAnAlertAndAnAmberFlag()
    {
        await RegisterManyAsync("10", 10);

        var odd = await RegisterAsync("AB-1234");

        odd.HasOpenCardAlert.ShouldBeTrue();
        _beneficiaries.ShouldContain(b => b.CardNumber.Value == "AB-1234");
        var alert = Alerts.ShouldHaveSingleItem();
        alert.CardNumber.ShouldBe("AB-1234");
        alert.CreatedByUserId.ShouldBe(_operator.Id);
        alert.WorkstationId.ShouldBe(1);
        _uowFactory.CurrentUow.AuditLogRows.ShouldContain(a => a.Action == "CardPatternAlert.Raised" && a.DetailsJson!.Contains("AB-1234"));
    }

    [Fact]
    public async Task Register_UsualNumber_RaisesNoAlert()
    {
        await RegisterManyAsync("10", 10);

        (await RegisterAsync("109999")).HasOpenCardAlert.ShouldBeFalse();
        Alerts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Register_FiveAlertsOfOneShape_ProposeANewSeriesThatStillAlertsUntilApproved()
    {
        await RegisterManyAsync("10", 10);
        for (var i = 1; i <= 5; i++)
        {
            await RegisterAsync($"AB-{i:D4}");
        }

        var series = Patterns.Single(p => p.Source == CardPatternSource.NewSeries);
        series.Shape.ShouldBe("LL-DDDD");
        series.Prefix.ShouldBe("AB");
        series.SampleCount.ShouldBe(5);

        (await RegisterAsync("AB-0006")).HasOpenCardAlert.ShouldBeTrue("a forger cannot teach the system a pattern");
        Patterns.Count(p => p.Source == CardPatternSource.NewSeries).ShouldBe(1);
    }

    [Fact]
    public async Task ApprovePattern_NewSeries_ClosesItsAlertsAndStopsAlerting()
    {
        await RegisterManyAsync("10", 10);
        for (var i = 1; i <= 5; i++)
        {
            await RegisterAsync($"AB-{i:D4}");
        }

        var series = Patterns.Single(p => p.Source == CardPatternSource.NewSeries);
        _currentUser.SetUser(_admin);

        var closed = await _patterns.ApprovePatternAsync(series.Id);

        closed.Value.ShouldBe(5);
        series.Status.ShouldBe(CardPatternStatus.Approved);
        series.DecidedByUserId.ShouldBe(_admin.Id);
        Alerts.ShouldAllBe(a => a.Status == CardPatternAlertStatus.Reviewed);
        _uowFactory.CurrentUow.AuditLogRows.ShouldContain(a => a.Action == "CardPattern.Approved");

        _currentUser.SetUser(_operator);
        (await RegisterAsync("AB-0007")).HasOpenCardAlert.ShouldBeFalse();
    }

    [Fact]
    public async Task RejectPattern_KeepsAlerting()
    {
        await RegisterManyAsync("10", 10);
        _currentUser.SetUser(_admin);
        (await _patterns.RejectPatternAsync(Patterns.Single().Id)).IsSuccess.ShouldBeTrue();
        _currentUser.SetUser(_operator);

        (await RegisterAsync("109999")).HasOpenCardAlert.ShouldBeTrue();
    }

    [Fact]
    public async Task ApprovePattern_Twice_SaysItWasDecided()
    {
        await RegisterManyAsync("10", 10);
        _currentUser.SetUser(_admin);
        await _patterns.ApprovePatternAsync(Patterns.Single().Id);

        (await _patterns.RejectPatternAsync(Patterns.Single().Id)).Error.ShouldBe(Errors.CardPattern.AlreadyDecided);
    }

    [Fact]
    public async Task AdminUseCases_AsSupervisor_AreForbidden()
    {
        _currentUser.SetUser(3, "sup", "مشرف", UserRole.Supervisor);

        (await _patterns.GetPatternsAsync()).Error.ShouldBe(Errors.General.Forbidden);
        (await _patterns.GetAlertsAsync(true)).Error.ShouldBe(Errors.General.Forbidden);
        (await _patterns.ApprovePatternAsync(1)).Error.ShouldBe(Errors.General.Forbidden);
        (await _patterns.ReviewAlertAsync(1, null)).Error.ShouldBe(Errors.General.Forbidden);
    }

    [Fact]
    public async Task ReviewAlert_RecordsTheAdminAndNote()
    {
        await RegisterManyAsync("10", 10);
        var odd = await RegisterAsync("AB-1234");
        _currentUser.SetUser(_admin);

        var alerts = (await _patterns.GetAlertsAsync(openOnly: true)).Value;
        alerts.ShouldHaveSingleItem().HolderName.ShouldBe(odd.FullName);
        alerts[0].CreatedByName.ShouldBe("موظف الشباك");
        alerts[0].WorkstationName.ShouldBe("الشباك 1");

        (await _patterns.ReviewAlertAsync(alerts[0].Id, "تأكدنا من البطاقة مع الصندوق")).IsSuccess.ShouldBeTrue();

        var alert = Alerts.Single();
        alert.Status.ShouldBe(CardPatternAlertStatus.Reviewed);
        alert.ReviewedByUserId.ShouldBe(_admin.Id);
        alert.ReviewNote.ShouldBe("تأكدنا من البطاقة مع الصندوق");
        (await _patterns.GetAlertsAsync(openOnly: true)).Value.ShouldBeEmpty();
        (await _patterns.ReviewAlertAsync(alert.Id, null)).Error.ShouldBe(Errors.CardPattern.AlertAlreadyReviewed);
    }

    [Fact]
    public async Task ReviewAlert_NoteTooLong_IsRefused()
    {
        _currentUser.SetUser(_admin);

        (await _patterns.ReviewAlertAsync(1, new string('x', 251))).Error.ShouldBe(Errors.CardPattern.NoteTooLong);
    }

    [Fact]
    public async Task GetPatterns_ShowsAReadableForm()
    {
        await RegisterManyAsync("10", 10);
        _currentUser.SetUser(_admin);

        var row = (await _patterns.GetPatternsAsync()).Value.ShouldHaveSingleItem();

        row.Display.ShouldBe("10####");
        row.OrganizationName.ShouldBe("صندوق التضامن");
        row.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Update_ChangedToAnUnusualNumber_RaisesAnAlert()
    {
        await RegisterManyAsync("10", 10);
        var holder = _beneficiaries[0];
        _currentUser.SetUser(_admin);

        var result = await _beneficiaries_.UpdateAsync(new UpdateBeneficiaryRequest(
            holder.Id, holder.OrganizationId, "XY-9999", holder.FullName, holder.Phone!.Value));

        result.Value.HasOpenCardAlert.ShouldBeTrue();
        Alerts.ShouldHaveSingleItem().CardNumber.ShouldBe("XY-9999");
    }

    [Fact]
    public async Task Lookup_HolderWithAnOpenAlert_CarriesTheAmberFlag()
    {
        await RegisterManyAsync("10", 10);
        await RegisterAsync("AB-1234");

        var lookup = await _beneficiaries_.LookupByCardNumberAsync("AB-1234");

        lookup.Value.Matches.Single().Beneficiary.HasOpenCardAlert.ShouldBeTrue();
    }

    [Fact]
    public async Task Settings_StoredValues_AreUsed()
    {
        _uowFactory.CurrentUow.AppSettings.ShouldBeOfType<FakeAppSettingRepository>()
            .Add(new AppSetting("CardPattern.MinSample", "3", DateTime.UtcNow, null));

        await RegisterManyAsync("10", 3);

        Patterns.ShouldHaveSingleItem();
    }

    private async Task RegisterManyAsync(string prefix, int count)
    {
        for (var i = 1; i <= count; i++)
        {
            await RegisterAsync($"{prefix}{i:D4}");
        }
    }

    private async Task<BeneficiaryDto> RegisterAsync(string card)
    {
        _phone++;
        var result = await _beneficiaries_.RegisterAsync(new RegisterBeneficiaryRequest(_org.Id, card, $"زائر {card}", $"0912{_phone:D6}"));
        result.IsSuccess.ShouldBeTrue(result.IsFailure ? result.Error.Code : null);
        return result.Value;
    }
}
