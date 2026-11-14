using ParkGate.Application.Common;
using ParkGate.Application.GiftVouchers.DTOs;

namespace ParkGate.Application.GiftVouchers.Services;

public interface IGiftVoucherService
{
    Task<Result<GiftEligibilityDto>> GetEligibilityAsync(Guid beneficiaryId, CancellationToken ct = default);

    Task<Result<IssuedGiftVoucherDto>> IssueAsync(IssueGiftVoucherRequest request, CancellationToken ct = default);

    Task<Result<IssuedGiftVoucherDto>> IssueOverrideAsync(IssueGiftOverrideRequest request, CancellationToken ct = default);

    Task<Result<GiftRedemptionDto>> RedeemAsync(string? scannedText, CancellationToken ct = default);

    Task<Result> VoidAsync(VoidGiftVoucherRequest request, string? approvalToken = null, CancellationToken ct = default);
}
