using ParkGate.Domain.CardPatterns;
using ParkGate.Domain.Entities;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.Policies;

/// <summary>F8, BR-29: card-number shapes, patterns and what the system proposes.</summary>
public sealed class CardPatternTests
{
    private static readonly DateTime Now = new(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly CardPatternSettings Settings = CardPatternSettings.Default;

    [Theory]
    [InlineData("100200", "DDDDDD")]
    [InlineData("AB-1234", "LL-DDDD")]
    [InlineData("SSF 12", "LLLODD")]
    [InlineData("ص12", "LDD")]
    public void Shape_ClassifiesEachCharacter(string card, string shape) => CardShape.Of(card).ShouldBe(shape);

    [Theory]
    [InlineData("DDDDDD", "10", "10####")]
    [InlineData("LL-DDDD", null, "AA-####")]
    [InlineData("LL-DDDD", "AB", "AB-####")]
    public void Describe_ShowsPrefixThenPlaceholders(string shape, string? prefix, string expected) =>
        CardShape.Describe(shape, prefix).ShouldBe(expected);

    [Fact]
    public void Check_NoPatternYet_IsLearning() =>
        CardPatternLearner.Check("100200", []).ShouldBe(CardPatternVerdict.Learning);

    [Theory]
    [InlineData("100200", CardPatternVerdict.Matches)]
    [InlineData("109999", CardPatternVerdict.Matches)]
    [InlineData("200200", CardPatternVerdict.Mismatch)]  // wrong prefix
    [InlineData("1002001", CardPatternVerdict.Mismatch)] // wrong length
    [InlineData("AB-1234", CardPatternVerdict.Mismatch)]
    public void Check_AgainstAnApprovedPattern(string card, CardPatternVerdict expected)
    {
        var pattern = OrganizationCardPattern.Propose(1, "DDDDDD", "10", CardPatternSource.NewSeries, 12, Now);
        pattern.Approve(1, Now);

        CardPatternLearner.Check(card, [pattern]).ShouldBe(expected);
    }

    [Fact]
    public void Check_DominantProposal_IsUsedUntilTheAdminDecides()
    {
        var pattern = OrganizationCardPattern.Propose(1, "DDDDDD", null, CardPatternSource.Dominant, 12, Now);

        CardPatternLearner.Check("123456", [pattern]).ShouldBe(CardPatternVerdict.Matches);
    }

    [Fact]
    public void Check_NewSeriesProposal_KeepsAlertingUntilApproved()
    {
        var dominant = OrganizationCardPattern.Propose(1, "DDDDDD", null, CardPatternSource.Dominant, 12, Now);
        var series = OrganizationCardPattern.Propose(1, "LL-DDDD", null, CardPatternSource.NewSeries, 5, Now);

        CardPatternLearner.Check("AB-1234", [dominant, series]).ShouldBe(CardPatternVerdict.Mismatch);

        series.Approve(1, Now);
        CardPatternLearner.Check("AB-1234", [dominant, series]).ShouldBe(CardPatternVerdict.Matches);
    }

    [Fact]
    public void Check_RejectedPattern_KeepsAlerting()
    {
        var pattern = OrganizationCardPattern.Propose(1, "DDDDDD", null, CardPatternSource.Dominant, 12, Now);
        pattern.Reject(1, Now);

        CardPatternLearner.Check("123456", [pattern]).ShouldBe(CardPatternVerdict.Mismatch);
    }

    [Fact]
    public void ProposeDominant_TooFewCards_ProposesNothing() =>
        CardPatternLearner.ProposeDominant(Numbers("10", 9), Settings).ShouldBeNull();

    [Fact]
    public void ProposeDominant_TenCardsWithASharedPrefix_ProposesShapeAndPrefix()
    {
        var proposal = CardPatternLearner.ProposeDominant(Numbers("10", 10), Settings);

        proposal.ShouldBe(new ProposedCardPattern("DDDDDD", "10", 10));
    }

    [Fact]
    public void ProposeDominant_PrefixNotSharedByNinetyPercent_ProposesTheShapeOnly()
    {
        var cards = Numbers("10", 8).Concat(Numbers("20", 2)).ToList();

        CardPatternLearner.ProposeDominant(cards, Settings).ShouldBe(new ProposedCardPattern("DDDDDD", null, 10));
    }

    [Fact]
    public void ProposeDominant_NoShapeHoldsMoreThanHalf_ProposesNothing()
    {
        var cards = Numbers("10", 5).Concat(Enumerable.Range(0, 5).Select(i => $"AB-{i:D4}")).ToList();

        CardPatternLearner.ProposeDominant(cards, Settings).ShouldBeNull();
    }

    [Fact]
    public void ProposeDominant_MinorityShapesAreLeftOut()
    {
        var cards = Numbers("10", 9).Concat(["AB-0001", "AB-0002"]).ToList();

        CardPatternLearner.ProposeDominant(cards, Settings).ShouldBe(new ProposedCardPattern("DDDDDD", "10", 9));
    }

    [Fact]
    public void ProposeNewSeries_BelowTheThreshold_ProposesNothing() =>
        CardPatternLearner.ProposeNewSeries(["AB-0001", "AB-0002", "AB-0003", "AB-0004"], Settings).ShouldBeNull();

    [Fact]
    public void ProposeNewSeries_AtTheThreshold_ProposesIt() =>
        CardPatternLearner.ProposeNewSeries(["AB-0001", "AB-0002", "AB-0003", "AB-0004", "AB-0005"], Settings)
            .ShouldBe(new ProposedCardPattern("LL-DDDD", "AB", 5));

    [Fact]
    public void ProposeNewSeries_MixedShapes_Throws() =>
        Should.Throw<ArgumentException>(() => CardPatternLearner.ProposeNewSeries(["AB-0001", "1", "2", "3", "4"], Settings));

    [Fact]
    public void Pattern_OnlyAProposalCanBeDecided()
    {
        var pattern = OrganizationCardPattern.Propose(1, "DDDDDD", null, CardPatternSource.Dominant, 12, Now);
        pattern.Approve(7, Now);

        pattern.Status.ShouldBe(CardPatternStatus.Approved);
        pattern.DecidedByUserId.ShouldBe(7);
        Should.Throw<InvalidOperationException>(() => pattern.Reject(7, Now));
    }

    [Fact]
    public void Pattern_PrefixLongerThanTheShape_Throws() =>
        Should.Throw<ArgumentException>(() => OrganizationCardPattern.Propose(1, "DD", "123", CardPatternSource.NewSeries, 5, Now));

    [Fact]
    public void Alert_TakesTheShapeAndCanBeReviewedOnce()
    {
        var alert = CardPatternAlert.Raise(1, Guid.NewGuid(), "AB-1234", Now, 3, 1);

        alert.Shape.ShouldBe("LL-DDDD");
        alert.Status.ShouldBe(CardPatternAlertStatus.Open);

        alert.Review(9, Now, "  بطاقة جديدة من الصندوق  ");
        alert.Status.ShouldBe(CardPatternAlertStatus.Reviewed);
        alert.ReviewNote.ShouldBe("بطاقة جديدة من الصندوق");
        Should.Throw<InvalidOperationException>(() => alert.Review(9, Now, null));
    }

    [Fact]
    public void Settings_RejectNonsense() =>
        Should.Throw<ArgumentOutOfRangeException>(() => new CardPatternSettings(0, 5, 2, 90));

    private static List<string> Numbers(string prefix, int count) =>
        Enumerable.Range(0, count).Select(i => $"{prefix}{i:D4}").ToList();
}
