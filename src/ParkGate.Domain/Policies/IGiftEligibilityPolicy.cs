namespace ParkGate.Domain.Policies;

public interface IGiftEligibilityPolicy
{
    GiftEligibilityDecision Evaluate(GiftEligibilityContext context);
}
