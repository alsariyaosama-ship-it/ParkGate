using ParkGate.Domain.Entities;

namespace ParkGate.Domain.CardPatterns;

/// <summary>What a card number means for its organization's patterns (BR-29).</summary>
public enum CardPatternVerdict
{
    /// <summary>The organization has no pattern yet; nothing to compare with.</summary>
    Learning = 1,

    /// <summary>The number matches an active pattern.</summary>
    Matches = 2,

    /// <summary>The number matches no active pattern: an alert for the Admin.</summary>
    Mismatch = 3,
}

/// <summary>A pattern the system wants to propose.</summary>
public sealed record ProposedCardPattern(string Shape, string? Prefix, int SampleCount);

/// <summary>
/// Pure rules of the card-number pattern alerts (F8, BR-29). No clock, no storage: the Application layer loads the
/// numbers and patterns and stores what this class decides.
/// </summary>
public static class CardPatternLearner
{
    public static CardPatternVerdict Check(string cardNumber, IReadOnlyCollection<OrganizationCardPattern> patterns)
    {
        ArgumentNullException.ThrowIfNull(cardNumber);
        ArgumentNullException.ThrowIfNull(patterns);

        if (patterns.Count == 0)
        {
            return CardPatternVerdict.Learning;
        }

        return patterns.Any(p => p.IsActive && p.Matches(cardNumber))
            ? CardPatternVerdict.Matches
            : CardPatternVerdict.Mismatch;
    }

    /// <summary>
    /// The organization's first pattern, from its registered numbers: needs <see cref="CardPatternSettings.MinSample"/>
    /// numbers and one shape held by more than half of them.
    /// </summary>
    public static ProposedCardPattern? ProposeDominant(IReadOnlyCollection<string> cardNumbers, CardPatternSettings settings)
    {
        ArgumentNullException.ThrowIfNull(cardNumbers);
        ArgumentNullException.ThrowIfNull(settings);

        if (cardNumbers.Count < settings.MinSample)
        {
            return null;
        }

        var top = cardNumbers.GroupBy(CardShape.Of).OrderByDescending(g => g.Count()).First();
        if (top.Count() * 2 <= cardNumbers.Count)
        {
            return null;
        }

        return Proposal(top.Key, top.ToList(), settings);
    }

    /// <summary>
    /// A new series, from the open alerts of one shape: needs <see cref="CardPatternSettings.NewSeriesThreshold"/> of them.
    /// </summary>
    public static ProposedCardPattern? ProposeNewSeries(IReadOnlyCollection<string> alertedNumbersOfOneShape, CardPatternSettings settings)
    {
        ArgumentNullException.ThrowIfNull(alertedNumbersOfOneShape);
        ArgumentNullException.ThrowIfNull(settings);

        if (alertedNumbersOfOneShape.Count < settings.NewSeriesThreshold)
        {
            return null;
        }

        var shape = CardShape.Of(alertedNumbersOfOneShape.First());
        if (alertedNumbersOfOneShape.Any(n => CardShape.Of(n) != shape))
        {
            throw new ArgumentException("All numbers must have the same shape.", nameof(alertedNumbersOfOneShape));
        }

        return Proposal(shape, alertedNumbersOfOneShape.ToList(), settings);
    }

    private static ProposedCardPattern Proposal(string shape, IReadOnlyList<string> numbers, CardPatternSettings settings)
    {
        var prefix = CommonPrefix(numbers, settings);
        var sample = prefix is null ? numbers.Count : numbers.Count(n => n.StartsWith(prefix, StringComparison.Ordinal));
        return new ProposedCardPattern(shape, prefix, sample);
    }

    /// <summary>The first <see cref="CardPatternSettings.PrefixLength"/> characters, when enough numbers share them.</summary>
    internal static string? CommonPrefix(IReadOnlyList<string> numbers, CardPatternSettings settings)
    {
        if (settings.PrefixLength == 0 || numbers.Count == 0 || numbers.Any(n => n.Length <= settings.PrefixLength))
        {
            return null;
        }

        var top = numbers.GroupBy(n => n[..settings.PrefixLength], StringComparer.Ordinal).OrderByDescending(g => g.Count()).First();
        return top.Count() * 100 >= settings.PrefixSharePercent * numbers.Count ? top.Key : null;
    }
}
