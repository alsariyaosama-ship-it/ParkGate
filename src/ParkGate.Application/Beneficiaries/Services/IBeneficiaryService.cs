using ParkGate.Application.Beneficiaries.DTOs;
using ParkGate.Application.Common;

namespace ParkGate.Application.Beneficiaries.Services;

public interface IBeneficiaryService
{
    Task<Result<IReadOnlyList<BeneficiaryLookupResultDto>>> LookupByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryLookupResultDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryDto>> RegisterAsync(RegisterBeneficiaryRequest request, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryDto>> UpdateAsync(UpdateBeneficiaryRequest request, CancellationToken cancellationToken = default);
    Task<Result<BeneficiaryDto>> ChangeStatusAsync(ChangeBeneficiaryStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<BeneficiaryDto>>> SearchAsync(string searchTerm, int maxResults = 50, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<EntryAttemptDto>>> GetHistoryAsync(Guid beneficiaryId, int maxResults = 100, CancellationToken cancellationToken = default);
}
