using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkGate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RequeueGiftAndPolicyOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // R2.7: organizations and age categories gained gift and companion-limit fields after most
            // of them were first synced, and gift vouchers were never synced. Queue one current-state
            // row per existing record so the cloud catches up. The voucher token is deliberately left out.
            migrationBuilder.Sql(RequeueSql);
        }

        /// <summary>The data fix, exposed so an integration test can run it against real rows.</summary>
        public const string RequeueSql = @"
INSERT INTO [SyncOutboxes]
    ([Id], [EntityType], [EntityId], [OperationType], [PayloadJson], [SchemaVersion], [IsProcessed], [RetryCount], [CreatedAtUtc])
SELECT
    NEWID(), N'Organization', CONVERT(nvarchar(20), o.[Id]), N'Update',
    (SELECT
        o.[Id] AS [Id],
        o.[Code] AS [Code],
        o.[NameAr] AS [NameAr],
        o.[CooldownMinutes] AS [CooldownMinutes],
        o.[IsDefault] AS [IsDefault],
        o.[IsActive] AS [IsActive],
        o.[MaxFreeCompanions] AS [MaxFreeCompanions],
        o.[GiftEnabled] AS [GiftEnabled],
        o.[GiftValue] AS [GiftValue],
        o.[GiftIntervalDays] AS [GiftIntervalDays],
        CONVERT(nvarchar(30), o.[CreatedAtUtc], 127) + N'Z' AS [CreatedAtUtc],
        CONVERT(nvarchar(30), o.[UpdatedAtUtc], 127) + N'Z' AS [UpdatedAtUtc]
     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES),
    1, 0, 0, SYSUTCDATETIME()
FROM [Organizations] o;

INSERT INTO [SyncOutboxes]
    ([Id], [EntityType], [EntityId], [OperationType], [PayloadJson], [SchemaVersion], [IsProcessed], [RetryCount], [CreatedAtUtc])
SELECT
    NEWID(), N'AgeCategory', CONVERT(nvarchar(20), a.[Id]), N'Update',
    (SELECT
        a.[Id] AS [Id],
        a.[NameAr] AS [NameAr],
        a.[DescriptionAr] AS [DescriptionAr],
        a.[SortOrder] AS [SortOrder],
        a.[IsActive] AS [IsActive],
        a.[IsCompanionLimitExempt] AS [IsCompanionLimitExempt]
     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES),
    1, 0, 0, SYSUTCDATETIME()
FROM [AgeCategories] a;

INSERT INTO [SyncOutboxes]
    ([Id], [EntityType], [EntityId], [OperationType], [PayloadJson], [SchemaVersion], [IsProcessed], [RetryCount], [CreatedAtUtc])
SELECT
    NEWID(), N'GiftVoucher', CONVERT(nvarchar(20), g.[Id]), N'Update',
    (SELECT
        g.[Id] AS [Id],
        LOWER(CONVERT(nvarchar(36), g.[BeneficiaryId])) AS [BeneficiaryId],
        g.[OrganizationId] AS [OrganizationId],
        g.[Value] AS [Value],
        CONVERT(nvarchar(30), g.[IssuedAtUtc], 127) + N'Z' AS [IssuedAtUtc],
        CONVERT(nvarchar(10), g.[BusinessDate], 23) AS [BusinessDate],
        CONVERT(nvarchar(10), g.[ExpiresOn], 23) AS [ExpiresOn],
        g.[Status] AS [Status],
        g.[IssuedByUserId] AS [IssuedByUserId],
        g.[IssuedWorkstationId] AS [IssuedWorkstationId],
        g.[IsOverride] AS [IsOverride],
        g.[OverrideByUserId] AS [OverrideByUserId],
        g.[OverrideReason] AS [OverrideReason],
        CONVERT(nvarchar(30), g.[RedeemedAtUtc], 127) + N'Z' AS [RedeemedAtUtc],
        CONVERT(nvarchar(10), g.[RedeemedBusinessDate], 23) AS [RedeemedBusinessDate],
        g.[RedeemedByUserId] AS [RedeemedByUserId],
        g.[RedeemedWorkstationId] AS [RedeemedWorkstationId],
        CONVERT(nvarchar(30), g.[VoidedAtUtc], 127) + N'Z' AS [VoidedAtUtc],
        g.[VoidedByUserId] AS [VoidedByUserId],
        g.[VoidReason] AS [VoidReason]
     FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES),
    1, 0, 0, SYSUTCDATETIME()
FROM [GiftVouchers] g;";

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data-only migration: queued rows are ordinary outbox rows and are not undone.
        }
    }
}
