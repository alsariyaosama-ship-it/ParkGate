using ParkGate.Domain.EntryCards;

namespace ParkGate.Application.EntryCards;

/// <summary>اسم الجهاز الحالي (للسجل). يُسجَّل في DI من الإعدادات أو Environment.MachineName.</summary>
public sealed record GateStation(string Name);

public sealed record EntryCardOptions
{
    /// <summary>مدة صلاحية البطاقة الافتراضية عند الإصدار.</summary>
    public int ValidityDays { get; init; } = 365;
}

public sealed record IssuedEntryCard(Guid BeneficiaryId, Guid CardId, DateOnly ExpiresOn, string QrText);

public sealed record CardScanResult(CardScanStatus Status, Guid? BeneficiaryId)
{
    public bool IsAccepted => Status == CardScanStatus.Accepted;
    public string MessageAr => Status.ToArabic();
}
