using System.Text;
using ParkGate.Domain.EntryCards;
using Shouldly;
using Xunit;

namespace ParkGate.Domain.Tests.EntryCards;

public sealed class QrTokenFormatPurposeTests
{
    [Fact]
    public void MessageToSign_EntryCard_EqualsLegacyBytes()
    {
        byte[] body = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20];
        var expected = Encoding.ASCII.GetBytes("ParkGate.EntryCard/v1").Concat(body).ToArray();

        var actual = QrTokenFormat.MessageToSign(body, QrPurpose.EntryCard);

        actual.ShouldBe(expected);
    }

    [Fact]
    public void MessageToSign_DefaultParam_EqualsEntryCard()
    {
        byte[] body = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20];

        var defaultActual = QrTokenFormat.MessageToSign(body);
        var entryCardActual = QrTokenFormat.MessageToSign(body, QrPurpose.EntryCard);

        defaultActual.ShouldBe(entryCardActual);
    }

    [Fact]
    public void MessageToSign_GiftVoucher_DifferentFromEntryCard()
    {
        byte[] body = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20];

        var entryCard = QrTokenFormat.MessageToSign(body, QrPurpose.EntryCard);
        var giftVoucher = QrTokenFormat.MessageToSign(body, QrPurpose.GiftVoucher);

        giftVoucher.ShouldNotBe(entryCard);
        giftVoucher.ShouldBe(Encoding.ASCII.GetBytes("ParkGate.GiftVoucher/v1").Concat(body).ToArray());
    }
}

