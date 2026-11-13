using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PurgeSensitiveBeneficiaryOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // R2.6 (Q-R2): phone and national ID must never leave the park. Pending Beneficiary rows
            // may still carry (encrypted) copies of those fields, so they are dropped and replaced
            // by one clean row per holder. Delete operations only need the key, so their payload is
            // stripped in place instead.
            migrationBuilder.Sql(PurgeSql);
        }

        /// <summary>The data fix, exposed so an integration test can run it against real rows.</summary>
        public const string PurgeSql = @"
DELETE FROM [SyncOutboxes]
WHERE [EntityType] = N'Beneficiary' AND [IsProcessed] = 0 AND [OperationType] <> N'Delete';

UPDATE [SyncOutboxes]
SET [PayloadJson] = JSON_MODIFY(JSON_MODIFY([PayloadJson], '$.Phone', NULL), '$.NationalId', NULL)
WHERE [EntityType] = N'Beneficiary' AND [IsProcessed] = 0 AND [OperationType] = N'Delete';

INSERT INTO [SyncOutboxes]
    ([Id], [EntityType], [EntityId], [OperationType], [PayloadJson], [SchemaVersion], [IsProcessed], [RetryCount], [CreatedAtUtc])
SELECT
    NEWID(),
    N'Beneficiary',
    LOWER(CONVERT(nvarchar(36), b.[Id])),
    N'Update',
    (SELECT
        LOWER(CONVERT(nvarchar(36), b.[Id])) AS [Id],
        b.[OrganizationId] AS [OrganizationId],
        b.[CardNumber] AS [CardNumber],
        b.[FullName] AS [FullName],
        CONVERT(nvarchar(10), b.[CardExpiryDate], 23) AS [CardExpiryDate],
        b.[Status] AS [Status],
        b.[StatusReason] AS [StatusReason],
        b.[Notes] AS [Notes],
        CONVERT(nvarchar(30), b.[LastGrantedEntryUtc], 127) + N'Z' AS [LastGrantedEntryUtc],
        CONVERT(nvarchar(30), b.[CreatedAtUtc], 127) + N'Z' AS [CreatedAtUtc],
        b.[CreatedByUserId] AS [CreatedByUserId],
        CONVERT(nvarchar(30), b.[UpdatedAtUtc], 127) + N'Z' AS [UpdatedAtUtc],
        b.[UpdatedByUserId] AS [UpdatedByUserId]
     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES),
    1,
    0,
    0,
    SYSUTCDATETIME()
FROM [Beneficiaries] b;";

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only migration: the removed outbox rows cannot be restored, and must not be.
        }
    }
}
