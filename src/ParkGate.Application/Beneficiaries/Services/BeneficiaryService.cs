using FluentValidation;
using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.CardPatterns;
using ParkGate.Application.Common;
using ParkGate.Application.Ports.Persistence;
using ParkGate.Application.Security;
using ParkGate.Domain.Entities;
using ParkGate.Domain.Enums;
using ParkGate.Domain.Policies;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Application.Beneficiaries.Services;

public sealed class BeneficiaryService : IBeneficiaryService
{
    private readonly IUnitOfWorkFactory _uowFactory;
    private readonly IEntryEligibilityPolicy _eligibilityPolicy;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentWorkstation _currentWorkstation;
    private readonly IValidator<RegisterBeneficiaryRequest> _registerValidator;
    private readonly IValidator<UpdateBeneficiaryRequest> _updateValidator;
    private readonly IValidator<ChangeBeneficiaryStatusRequest> _statusValidator;

    public BeneficiaryService(
        IUnitOfWorkFactory uowFactory,
        IEntryEligibilityPolicy eligibilityPolicy,
        ICurrentUser currentUser,
        ICurrentWorkstation currentWorkstation,
        IValidator<RegisterBeneficiaryRequest> registerValidator,
        IValidator<UpdateBeneficiaryRequest> updateValidator,
        IValidator<ChangeBeneficiaryStatusRequest> statusValidator)
    {
        _uowFactory = uowFactory;
        _eligibilityPolicy = eligibilityPolicy;
        _currentUser = currentUser;
        _currentWorkstation = currentWorkstation;
        _registerValidator = registerValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
    }

    public async Task<Result<CardLookupDto>> LookupByCardNumberAsync(
        string cardNumber,
        int? organizationId = null,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.CheckIn))
        {
            return Errors.General.Forbidden;
        }

        if (!CardNumber.TryCreate(cardNumber, out var cardVo))
        {
            return Errors.Beneficiary.CardNotFound;
        }

        await using var uow = _uowFactory.Create();
        var beneficiaries = await uow.Beneficiaries.GetByCardNumberAsync(cardVo, cancellationToken);
        if (beneficiaries.Count == 0)
        {
            return Errors.Beneficiary.CardNotFound;
        }

        var (defaultCooldown, timeZoneId) = await GetSettingsAsync(uow, cancellationToken);
        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);
        var localToday = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var alerted = await uow.CardPatternAlerts.GetBeneficiariesWithOpenAlertsAsync(beneficiaries.Select(b => b.Id).ToList(), cancellationToken);
        var cityNames = await LoadCityNamesAsync(uow, cancellationToken);
        var matches = new List<BeneficiaryLookupResultDto>();
        var registeredElsewhere = new List<OrganizationRefDto>();
        foreach (var beneficiary in beneficiaries)
        {
            var org = await uow.Organizations.GetByIdAsync(beneficiary.OrganizationId, cancellationToken);
            var orgName = org?.NameAr ?? string.Empty;

            if (organizationId.HasValue && beneficiary.OrganizationId != organizationId.Value)
            {
                registeredElsewhere.Add(new OrganizationRefDto(beneficiary.OrganizationId, orgName));
                continue;
            }

            var preview = BuildEligibilityPreview(org, beneficiary, defaultCooldown, nowUtc, localToday);
            var lastCompanions = await GetLastCompanionsAsync(uow, beneficiary.Id, cancellationToken);

            matches.Add(new BeneficiaryLookupResultDto(ToViewerDto(beneficiary, orgName, CityNameOf(cityNames, beneficiary.CityId), alerted.Contains(beneficiary.Id)), preview, lastCompanions));
        }

        return new CardLookupDto(matches, registeredElsewhere);
    }

    public async Task<Result<BeneficiaryLookupResultDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.CheckIn))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var beneficiary = await uow.Beneficiaries.GetByIdAsync(id, cancellationToken);
        if (beneficiary is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        var org = await uow.Organizations.GetByIdAsync(beneficiary.OrganizationId, cancellationToken);
        var orgName = org?.NameAr ?? string.Empty;

        var (defaultCooldown, timeZoneId) = await GetSettingsAsync(uow, cancellationToken);
        var nowUtc = await uow.GetDbUtcNowAsync(cancellationToken);
        var localToday = TimeZoneHelper.GetLocalBusinessDate(nowUtc, timeZoneId);

        var preview = BuildEligibilityPreview(org, beneficiary, defaultCooldown, nowUtc, localToday);
        var lastCompanions = await GetLastCompanionsAsync(uow, beneficiary.Id, cancellationToken);
        var alerted = await uow.CardPatternAlerts.GetBeneficiariesWithOpenAlertsAsync([beneficiary.Id], cancellationToken);
        var cityNames = await LoadCityNamesAsync(uow, cancellationToken);

        return new BeneficiaryLookupResultDto(ToViewerDto(beneficiary, orgName, CityNameOf(cityNames, beneficiary.CityId), alerted.Contains(beneficiary.Id)), preview, lastCompanions);
    }

    public async Task<Result<BeneficiaryDto>> RegisterAsync(
        RegisterBeneficiaryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.CheckIn))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        CardNumber.TryCreate(request.CardNumber, out var cardVo);
        PhoneNumber? phoneVo = null;
        if (!string.IsNullOrWhiteSpace(request.Phone) && PhoneNumber.TryCreate(request.Phone, out var parsedPhone))
        {
            phoneVo = parsedPhone;
        }

        NationalId? natIdVo = null;
        if (!string.IsNullOrWhiteSpace(request.NationalId) && NationalId.TryCreate(request.NationalId, out var parsedNatId))
        {
            natIdVo = parsedNatId;
        }

        await using var uow = _uowFactory.Create();
        var org = await uow.Organizations.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        if (!org.IsActive)
        {
            return Errors.CheckIn.OrganizationInactive;
        }

        var city = await ResolveCityAsync(uow, request.CityId, currentCityId: null, cancellationToken);
        if (request.CityId is not null && city is null)
        {
            return Errors.City.NotFound;
        }

        var existingCard = await uow.Beneficiaries.GetByOrgAndCardAsync(request.OrganizationId, cardVo!, cancellationToken);
        if (existingCard is not null)
        {
            return Errors.Beneficiary.CardAlreadyExists;
        }

        if (natIdVo is not null && await uow.Beneficiaries.ExistsNationalIdAsync(natIdVo, null, cancellationToken))
        {
            return Errors.Beneficiary.NationalIdAlreadyExists;
        }

        var beneficiary = new Beneficiary(
            Guid.CreateVersion7(),
            request.OrganizationId,
            cardVo!,
            request.FullName,
            phoneVo,
            natIdVo,
            request.CardExpiryDate,
            request.Notes,
            request.CityId);

        uow.Beneficiaries.Add(beneficiary);
        await uow.AddAuditAsync(
            action: "BeneficiaryRegistered",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Beneficiary),
            entityId: beneficiary.Id.ToString(),
            cancellationToken: cancellationToken);

        // F8, BR-29: never blocks the registration; an unusual number only raises an alert for the Admin.
        var alerted = await CardPatternMonitor.CheckAsync(uow, beneficiary, _currentUser.UserId, _currentWorkstation.WorkstationId, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return ToViewerDto(beneficiary, org.NameAr, city?.NameAr, alerted);
    }

    public async Task<Result<BeneficiaryDto>> UpdateAsync(
        UpdateBeneficiaryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.EditBeneficiary))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        CardNumber.TryCreate(request.CardNumber, out var cardVo);
        PhoneNumber? phoneVo = null;
        if (!string.IsNullOrWhiteSpace(request.Phone) && PhoneNumber.TryCreate(request.Phone, out var parsedPhone))
        {
            phoneVo = parsedPhone;
        }

        NationalId? natIdVo = null;
        if (!string.IsNullOrWhiteSpace(request.NationalId) && NationalId.TryCreate(request.NationalId, out var parsedNatId))
        {
            natIdVo = parsedNatId;
        }

        await using var uow = _uowFactory.Create();
        var beneficiary = await uow.Beneficiaries.GetByIdAsync(request.Id, cancellationToken);
        if (beneficiary is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        var org = await uow.Organizations.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (org is null)
        {
            return Errors.Organization.NotFound;
        }

        // An inactive city is kept as is, but cannot be newly chosen.
        var city = await ResolveCityAsync(uow, request.CityId, beneficiary.CityId, cancellationToken);
        if (request.CityId is not null && city is null)
        {
            return Errors.City.NotFound;
        }

        string? cardChange = null;
        if (beneficiary.CardNumber != cardVo)
        {
            cardChange = AuditDetails.ToJson(new { OldCardNumber = beneficiary.CardNumber.Value, NewCardNumber = cardVo!.Value });
            var existingCard = await uow.Beneficiaries.GetByOrgAndCardAsync(request.OrganizationId, cardVo!, cancellationToken);
            if (existingCard is not null && existingCard.Id != beneficiary.Id)
            {
                return Errors.Beneficiary.CardAlreadyExists;
            }

            beneficiary.ChangeCardNumber(cardVo!);
        }

        if (natIdVo is not null && await uow.Beneficiaries.ExistsNationalIdAsync(natIdVo, beneficiary.Id, cancellationToken))
        {
            return Errors.Beneficiary.NationalIdAlreadyExists;
        }

        beneficiary.UpdateDetails(
            request.FullName,
            phoneVo,
            natIdVo,
            request.CardExpiryDate,
            request.Notes,
            request.CityId);

        await uow.AddAuditAsync(
            action: "BeneficiaryUpdated",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Beneficiary),
            entityId: beneficiary.Id.ToString(),
            detailsJson: cardChange,
            cancellationToken: cancellationToken);

        // F8, BR-29: a changed card number is checked like a new one.
        var alerted = cardChange is not null &&
            await CardPatternMonitor.CheckAsync(uow, beneficiary, _currentUser.UserId, _currentWorkstation.WorkstationId, cancellationToken);
        alerted |= (await uow.CardPatternAlerts.GetBeneficiariesWithOpenAlertsAsync([beneficiary.Id], cancellationToken)).Contains(beneficiary.Id);

        await uow.SaveChangesAsync(cancellationToken);

        return ToViewerDto(beneficiary, org.NameAr, city?.NameAr, alerted);
    }

    public async Task<Result<BeneficiaryDto>> ChangeStatusAsync(
        ChangeBeneficiaryStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.ChangeBeneficiaryStatus))
        {
            return Errors.General.Forbidden;
        }

        var validation = await _statusValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        await using var uow = _uowFactory.Create();
        var beneficiary = await uow.Beneficiaries.GetByIdAsync(request.Id, cancellationToken);
        if (beneficiary is null)
        {
            return Errors.Beneficiary.NotFound;
        }

        switch (request.TargetStatus)
        {
            case BeneficiaryStatus.Active:
                beneficiary.Reactivate();
                break;
            case BeneficiaryStatus.Suspended:
                beneficiary.Suspend(request.Reason);
                break;
            case BeneficiaryStatus.Blocked:
                beneficiary.Block(request.Reason);
                break;
            default:
                return Errors.Beneficiary.InvalidStatusTransition;
        }

        var org = await uow.Organizations.GetByIdAsync(beneficiary.OrganizationId, cancellationToken);
        var cityName = beneficiary.CityId is { } cityId
            ? (await uow.Cities.GetByIdAsync(cityId, cancellationToken))?.NameAr
            : null;

        await uow.AddAuditAsync(
            action: "BeneficiaryStatusChanged",
            userId: _currentUser.UserId,
            workstationId: _currentWorkstation.WorkstationId,
            entityName: nameof(Beneficiary),
            entityId: beneficiary.Id.ToString(),
            detailsJson: AuditDetails.ToJson(new { NewStatus = request.TargetStatus.ToString(), request.Reason }),
            cancellationToken: cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return ToViewerDto(beneficiary, org?.NameAr ?? string.Empty, cityName);
    }

    public async Task<Result<IReadOnlyList<BeneficiaryDto>>> SearchAsync(
        string searchTerm,
        int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasAny(Permission.CheckIn, Permission.EditBeneficiary, Permission.ChangeBeneficiaryStatus))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var matches = await uow.Beneficiaries.SearchAsync(searchTerm, maxResults, cancellationToken);
        var orgs = await uow.Organizations.GetAllAsync(activeOnly: false, cancellationToken);
        var orgLookup = orgs.ToDictionary(o => o.Id, o => o.NameAr);
        var cityNames = await LoadCityNamesAsync(uow, cancellationToken);

        var dtos = matches.Select(b =>
        {
            var orgName = orgLookup.TryGetValue(b.OrganizationId, out var name) ? name : string.Empty;
            return ToViewerDto(b, orgName, CityNameOf(cityNames, b.CityId));
        }).ToList();

        return dtos;
    }

    public async Task<Result<IReadOnlyList<EntryAttemptDto>>> GetHistoryAsync(
        Guid beneficiaryId,
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.Has(Permission.CheckIn))
        {
            return Errors.General.Forbidden;
        }

        await using var uow = _uowFactory.Create();
        var attempts = await uow.EntryAttempts.GetHistoryByBeneficiaryIdAsync(beneficiaryId, maxResults, cancellationToken);

        var dtos = attempts.Select(a => new EntryAttemptDto(
            a.Id,
            a.AttemptedAtUtc,
            a.BusinessDate,
            a.WorkstationId,
            a.UserId,
            a.BeneficiaryId,
            a.Result,
            a.DenialReason,
            a.OverrideReason,
            a.OverrideByUserId,
            a.TotalPersons,
            a.IsVoided,
            a.VoidReason,
            a.VoidedAtUtc,
            a.VoidedByUserId,
            a.Companions.Select(c => new CompanionCountDto(c.AgeCategoryId, string.Empty, c.Count)).ToList()
        )).ToList();

        return dtos;
    }

    /// <summary>SPEC §8, R6.16: operators get the national ID masked, so the full number never reaches their screen.</summary>
    private BeneficiaryDto ToViewerDto(Beneficiary beneficiary, string organizationName, string? cityName, bool hasOpenCardAlert = false)
    {
        var dto = beneficiary.ToDto(organizationName, cityName) with { HasOpenCardAlert = hasOpenCardAlert };
        return _currentUser.Has(Permission.ViewFullNationalId)
            ? dto
            : dto with { NationalId = NationalIdMask.Mask(dto.NationalId) };
    }

    /// <summary>
    /// Finds the chosen city. An inactive city is accepted only when it is the one the holder already has, so an old
    /// choice is never lost by an unrelated edit (BR-32).
    /// </summary>
    private static async Task<City?> ResolveCityAsync(
        IUnitOfWork uow,
        int? cityId,
        int? currentCityId,
        CancellationToken cancellationToken)
    {
        if (cityId is not { } id)
        {
            return null;
        }

        var city = await uow.Cities.GetByIdAsync(id, cancellationToken);
        return city is not null && (city.IsActive || id == currentCityId) ? city : null;
    }

    private static async Task<IReadOnlyDictionary<int, string>> LoadCityNamesAsync(
        IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        var cities = await uow.Cities.GetAllAsync(cancellationToken);
        return cities.ToDictionary(c => c.Id, c => c.NameAr);
    }

    private static string? CityNameOf(IReadOnlyDictionary<int, string> cityNames, int? cityId) =>
        cityId is { } id && cityNames.TryGetValue(id, out var name) ? name : null;

    private EligibilityPreviewDto BuildEligibilityPreview(
        Organization? organization,
        Beneficiary beneficiary,
        int defaultCooldownMinutes,
        DateTime nowUtc,
        DateOnly localToday)
    {
        if (organization is null)
        {
            return new EligibilityPreviewDto(
                IsEligible: false,
                Result: EntryResult.Denied,
                DenialReason: DenialReason.OrganizationInactive,
                DenialReasonText: GetDenialReasonArabic(DenialReason.OrganizationInactive),
                CanBeOverridden: false,
                EffectiveCooldownMinutes: defaultCooldownMinutes,
                LastGrantedEntryUtc: beneficiary.LastGrantedEntryUtc,
                NextAllowedEntryUtc: null,
                RemainingCooldownMinutes: null);
        }

        var context = new EligibilityContext(organization, beneficiary, defaultCooldownMinutes, nowUtc, localToday);
        var decision = _eligibilityPolicy.Evaluate(context);

        int? remainingMinutes = null;
        if (decision.NextEligibleAtUtc.HasValue && decision.NextEligibleAtUtc.Value > nowUtc)
        {
            remainingMinutes = (int)Math.Ceiling((decision.NextEligibleAtUtc.Value - nowUtc).TotalMinutes);
        }

        return new EligibilityPreviewDto(
            IsEligible: decision.IsEligible,
            Result: decision.IsEligible ? EntryResult.Allowed : EntryResult.Denied,
            DenialReason: decision.DenialReason,
            DenialReasonText: GetDenialReasonArabic(decision.DenialReason),
            CanBeOverridden: decision.CanBeOverridden,
            EffectiveCooldownMinutes: decision.EffectiveCooldownMinutes,
            LastGrantedEntryUtc: beneficiary.LastGrantedEntryUtc,
            NextAllowedEntryUtc: decision.NextEligibleAtUtc,
            RemainingCooldownMinutes: remainingMinutes,
            AllowedEntryDays: organization.AllowedEntryDays);
    }

    private static string GetDenialReasonArabic(DenialReason? reason) => reason switch
    {
        DenialReason.CooldownActive => "فترة التهدئة نشطة (سبق الدخول)",
        DenialReason.CardExpired => "صلاحية البطاقة منتهية",
        DenialReason.HolderSuspended => "حساب الزبون موقوف مؤقتاً",
        DenialReason.HolderBlocked => "حساب الزبون محظور",
        DenialReason.OrganizationInactive => "الجهة الراعية غير نشطة",
        DenialReason.DayNotAllowed => "اليوم ليس من أيام دخول الجهة",
        _ => string.Empty
    };

    private static async Task<(int DefaultCooldownMinutes, string TimeZoneId)> GetSettingsAsync(
        IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        int defaultCooldown = 720;
        var cdSetting = await uow.AppSettings.GetByKeyAsync("Entry.DefaultCooldownMinutes", cancellationToken);
        if (cdSetting is not null && int.TryParse(cdSetting.Value, out var parsedCd))
        {
            defaultCooldown = parsedCd;
        }

        string timeZoneId = "Libya Standard Time";
        var tzSetting = await uow.AppSettings.GetByKeyAsync("Park.TimeZoneId", cancellationToken);
        if (tzSetting is not null && !string.IsNullOrWhiteSpace(tzSetting.Value))
        {
            timeZoneId = tzSetting.Value;
        }

        return (defaultCooldown, timeZoneId);
    }

    private static async Task<IReadOnlyList<CompanionCountDto>> GetLastCompanionsAsync(
        IUnitOfWork uow,
        Guid beneficiaryId,
        CancellationToken cancellationToken)
    {
        var latest = await uow.EntryAttempts.GetLatestAttemptForBeneficiaryAsync(beneficiaryId, cancellationToken);
        if (latest is null || latest.Result == EntryResult.Denied || latest.IsVoided)
        {
            return Array.Empty<CompanionCountDto>();
        }

        return latest.Companions
            .Select(c => new CompanionCountDto(c.AgeCategoryId, string.Empty, c.Count))
            .ToList();
    }
}
