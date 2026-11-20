namespace ParkGate.Domain.CardPatterns;

/// <summary>Tuning of the card-number pattern alerts (BR-29, DATABASE.md §5 <c>CardPattern.*</c>).</summary>
public sealed record CardPatternSettings
{
    public const int DefaultMinSample = 10;
    public const int DefaultNewSeriesThreshold = 5;
    public const int DefaultPrefixLength = 2;
    public const int DefaultPrefixSharePercent = 90;

    public static CardPatternSettings Default { get; } =
        new(DefaultMinSample, DefaultNewSeriesThreshold, DefaultPrefixLength, DefaultPrefixSharePercent);

    public CardPatternSettings(int minSample, int newSeriesThreshold, int prefixLength, int prefixSharePercent)
    {
        if (minSample < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(minSample));
        }

        if (newSeriesThreshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(newSeriesThreshold));
        }

        if (prefixLength < 0 || prefixLength > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(prefixLength));
        }

        if (prefixSharePercent < 1 || prefixSharePercent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(prefixSharePercent));
        }

        MinSample = minSample;
        NewSeriesThreshold = newSeriesThreshold;
        PrefixLength = prefixLength;
        PrefixSharePercent = prefixSharePercent;
    }

    /// <summary>Registered cards needed before the first pattern is proposed.</summary>
    public int MinSample { get; }

    /// <summary>Open alerts with one shape before that shape is proposed as a new series.</summary>
    public int NewSeriesThreshold { get; }

    /// <summary>Characters compared for a shared prefix; 0 turns prefixes off.</summary>
    public int PrefixLength { get; }

    /// <summary>Share of the numbers that must start with the prefix for it to be part of the pattern.</summary>
    public int PrefixSharePercent { get; }
}
