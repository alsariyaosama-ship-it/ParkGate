namespace ParkGate.Domain.Policies;

/// <summary>
/// Strategy contract for evaluating card holder entry eligibility.
/// </summary>
public interface IEntryEligibilityPolicy
{
    EligibilityDecision Evaluate(EligibilityContext context);
}
