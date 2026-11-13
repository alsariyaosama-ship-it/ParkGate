using ParkGate.Domain.Entities;
using Shouldly;

namespace ParkGate.Domain.Tests.Entities;

public sealed class SyncOutboxTests
{
    [Fact]
    public void Create_WithValidParameters_InitializesCorrectly()
    {
        var now = DateTime.UtcNow;
        var outbox = SyncOutbox.Create(
            entityType: "Beneficiary",
            entityId: "12345",
            operationType: "Insert",
            payloadJson: "{\"FullName\":\"Ahmed\"}",
            createdAtUtc: now,
            schemaVersion: 1);

        outbox.Id.ShouldNotBe(Guid.Empty);
        outbox.EntityType.ShouldBe("Beneficiary");
        outbox.EntityId.ShouldBe("12345");
        outbox.OperationType.ShouldBe("Insert");
        outbox.PayloadJson.ShouldBe("{\"FullName\":\"Ahmed\"}");
        outbox.SchemaVersion.ShouldBe(1);
        outbox.IsProcessed.ShouldBeFalse();
        outbox.RetryCount.ShouldBe(0);
        outbox.LastError.ShouldBeNull();
        outbox.CreatedAtUtc.ShouldBe(now);
        outbox.ProcessedAtUtc.ShouldBeNull();
    }

    [Fact]
    public void MarkProcessed_SetsFlagAndTimestamp_ClearsError()
    {
        var now = DateTime.UtcNow;
        var outbox = SyncOutbox.Create("Beneficiary", "1", "Update", "{}", now);
        outbox.RecordFailure("Some network error");

        var processedTime = now.AddMinutes(5);
        outbox.MarkProcessed(processedTime);

        outbox.IsProcessed.ShouldBeTrue();
        outbox.ProcessedAtUtc.ShouldBe(processedTime);
        outbox.LastError.ShouldBeNull();
    }

    [Fact]
    public void RecordFailure_IncrementsRetryCount_AndSetsTruncatedErrorIfTooLong()
    {
        var now = DateTime.UtcNow;
        var outbox = SyncOutbox.Create("Beneficiary", "1", "Update", "{}", now);

        outbox.RecordFailure("First failure");
        outbox.RetryCount.ShouldBe(1);
        outbox.LastError.ShouldBe("First failure");

        var longError = new string('A', 600);
        outbox.RecordFailure(longError);
        outbox.RetryCount.ShouldBe(2);
        outbox.LastError!.Length.ShouldBe(SyncOutbox.MaxLastErrorLength);
    }

    [Fact]
    public void PayloadJson_HasNoPublicWayToChangeItAfterCreation()
    {
        // R2.1: the payload is sent exactly as stored, so a retry can never double-transform it.
        typeof(SyncOutbox).GetProperty(nameof(SyncOutbox.PayloadJson))!.SetMethod!.IsPublic.ShouldBeFalse();
        typeof(SyncOutbox).GetMethods().ShouldNotContain(m => m.Name == "UpdatePayload");
    }

    [Theory]
    [InlineData("", "1", "Insert", "{}")]
    [InlineData("Entity", "", "Insert", "{}")]
    [InlineData("Entity", "1", "", "{}")]
    [InlineData("Entity", "1", "Insert", "")]
    public void Create_WithInvalidArguments_ThrowsException(string entityType, string entityId, string op, string payload)
    {
        Should.Throw<ArgumentException>(() =>
            SyncOutbox.Create(entityType, entityId, op, payload, DateTime.UtcNow));
    }
}
