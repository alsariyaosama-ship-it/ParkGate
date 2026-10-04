namespace ParkGate.Domain.Entities;

/// <summary>
/// Companion count breakdown by age category for an entry attempt (BR-07, BR-08, BR-09).
/// </summary>
public sealed class EntryAttemptCompanion
{
    public Guid EntryAttemptId { get; private set; }
    public int AgeCategoryId { get; private set; }
    public int Count { get; private set; }

    private EntryAttemptCompanion() { }

    public EntryAttemptCompanion(int ageCategoryId, int count)
    {
        if (ageCategoryId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ageCategoryId), "Age category Id must be positive.");
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Companion count cannot be negative.");
        }

        AgeCategoryId = ageCategoryId;
        Count = count;
    }

    internal void AttachToAttempt(Guid entryAttemptId)
    {
        EntryAttemptId = entryAttemptId;
    }
}
