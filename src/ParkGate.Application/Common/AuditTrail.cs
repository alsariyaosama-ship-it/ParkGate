using ParkGate.Domain.Entities;

namespace ParkGate.Application.Common;

/// <summary>
/// Common audit details for entry and gift operations (R5.18): who the visitor is, which business day, who did it on
/// the desk and, when a supervisor approved, who approved. <c>AuditLog</c> itself keeps the time (database clock),
/// the acting user and the workstation. Phone and national ID are never written (AGENTS.md §6).
/// </summary>
public static class AuditTrail
{
    public static Dictionary<string, object?> For(Beneficiary holder, DateOnly businessDate, int operatorUserId, int? approvedByUserId = null) =>
        new()
        {
            ["BeneficiaryId"] = holder.Id,
            ["CardNumber"] = holder.CardNumber.Value,
            ["HolderName"] = holder.FullName,
            ["OrganizationId"] = holder.OrganizationId,
            ["BusinessDate"] = businessDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            ["OperatorUserId"] = operatorUserId,
            ["ApprovedByUserId"] = approvedByUserId,
        };

    /// <summary>Adds one more detail and returns the same dictionary, for chaining.</summary>
    public static Dictionary<string, object?> With(this Dictionary<string, object?> details, string name, object? value)
    {
        details[name] = value;
        return details;
    }
}
