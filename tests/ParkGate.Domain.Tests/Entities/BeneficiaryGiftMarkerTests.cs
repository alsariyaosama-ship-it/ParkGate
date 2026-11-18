using ParkGate.Domain.Entities;
using ParkGate.Domain.ValueObjects;

namespace ParkGate.Domain.Tests.Entities;

/// <summary>R6.13: the holder's gift marker that makes two simultaneous gift issues conflict (ADR-026).</summary>
public sealed class BeneficiaryGiftMarkerTests
{
    private static readonly DateTime Issued = new(2026, 11, 18, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RecordGiftIssued_UtcTime_SetsMarker()
    {
        var holder = NewHolder();

        holder.RecordGiftIssued(Issued);

        holder.LastGiftIssuedUtc.ShouldBe(Issued);
    }

    [Fact]
    public void RecordGiftIssued_UnspecifiedKind_Throws()
    {
        var holder = NewHolder();

        Should.Throw<ArgumentException>(() => holder.RecordGiftIssued(DateTime.SpecifyKind(Issued, DateTimeKind.Unspecified)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RecalculateLastGiftIssued_SetsRemainingOrNull(bool hasRemaining)
    {
        var holder = NewHolder();
        holder.RecordGiftIssued(Issued.AddDays(5));

        holder.RecalculateLastGiftIssued(hasRemaining ? Issued : null);

        holder.LastGiftIssuedUtc.ShouldBe(hasRemaining ? Issued : null);
    }

    [Fact]
    public void RecalculateLastGiftIssued_UnspecifiedKind_Throws()
    {
        var holder = NewHolder();

        Should.Throw<ArgumentException>(() => holder.RecalculateLastGiftIssued(DateTime.SpecifyKind(Issued, DateTimeKind.Unspecified)));
    }

    private static Beneficiary NewHolder() =>
        new(Guid.NewGuid(), 1, CardNumber.Create("GM-1"), "حامل الهدية", PhoneNumber.Create("0912345678"));
}
