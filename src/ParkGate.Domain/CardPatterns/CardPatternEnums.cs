namespace ParkGate.Domain.CardPatterns;

/// <summary>DATABASE.md §2 (F8). Only an Admin moves a pattern out of <see cref="Proposed"/>.</summary>
public enum CardPatternStatus : byte
{
    Proposed = 1,
    Approved = 2,
    Rejected = 3,
}

/// <summary>DATABASE.md §2 (F8).</summary>
public enum CardPatternAlertStatus : byte
{
    Open = 1,
    Reviewed = 2,
}

/// <summary>
/// Why a pattern was proposed (F8). A <see cref="Dominant"/> proposal is the organization's first, learned from its
/// registered cards, and is used provisionally until the Admin decides. A <see cref="NewSeries"/> proposal comes from
/// repeated alerts and stops nothing until the Admin approves it, so a forger cannot teach the system a pattern.
/// </summary>
public enum CardPatternSource : byte
{
    Dominant = 1,
    NewSeries = 2,
}
