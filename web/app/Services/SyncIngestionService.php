<?php

namespace App\Services;

use App\Exceptions\SyncItemException;
use App\Models\AgeCategory;
use App\Models\Beneficiary;
use App\Models\EntryAttempt;
use App\Models\EntryAttemptCompanion;
use App\Models\GiftVoucher;
use App\Models\Organization;
use Carbon\CarbonImmutable;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class SyncIngestionService
{
    /** Parent tables first: foreign keys are checked inside the single batch transaction (R2.4). */
    private const DEPENDENCY_ORDER = [
        'Organization',
        'AgeCategory',
        'Beneficiary',
        'EntryAttempt',
        'EntryAttemptCompanion',
        'GiftVoucher',
    ];

    /**
     * Ingests a batch of outbox records sent by ParkGate.SyncAgent.
     */
    public function ingestBatch(array $records): array
    {
        $processed = 0;
        $failed = 0;

        DB::transaction(function () use ($records, &$processed, &$failed) {
            foreach (self::sortByDependency($records) as $item) {
                try {
                    $entityType = $item['entityType'] ?? null;
                    $opType = $item['operationType'] ?? 'Insert';
                    $payloadRaw = $item['payloadJson'] ?? null;

                    if (empty($entityType) || empty($payloadRaw)) {
                        continue;
                    }

                    $data = json_decode($payloadRaw, true);
                    if (! is_array($data)) {
                        // Rejected, not skipped: the agent must not delete a record we did not store.
                        throw new \InvalidArgumentException('payloadJson is not a JSON object.');
                    }

                    $this->processEntity($entityType, $opType, $data);
                    $processed++;
                } catch (\Throwable $e) {
                    $failed++;
                    // R1.6: never log the item; it holds personal data. The caller logs
                    // the entity type/id with an error id. Rethrow to roll back the batch.
                    throw new SyncItemException(
                        (string) ($item['entityType'] ?? 'unknown'),
                        (string) ($item['entityId'] ?? 'unknown'),
                        $e,
                    );
                }
            }
        });

        return [
            'processed' => $processed,
            'failed' => $failed,
        ];
    }

    /**
     * Upserts run parent-first; deletes run after them, child-first, so a delete never
     * meets a row that still references it. The sort is stable within a rank.
     *
     * @param  array<int, array<string, mixed>>  $records
     * @return array<int, array<string, mixed>>
     */
    public static function sortByDependency(array $records): array
    {
        $rank = fn (array $item): int => (int) array_search($item['entityType'] ?? null, self::DEPENDENCY_ORDER, true);
        $isDelete = fn (array $item): bool => ($item['operationType'] ?? null) === 'Delete';

        $indexed = [];
        foreach (array_values($records) as $position => $item) {
            $indexed[] = [$item, $position];
        }

        usort($indexed, function (array $a, array $b) use ($rank, $isDelete): int {
            [$itemA, $posA] = $a;
            [$itemB, $posB] = $b;

            $deleteA = $isDelete($itemA);
            $deleteB = $isDelete($itemB);
            if ($deleteA !== $deleteB) {
                return $deleteA <=> $deleteB;
            }

            $rankA = $rank($itemA);
            $rankB = $rank($itemB);
            if ($rankA !== $rankB) {
                return $deleteA ? $rankB <=> $rankA : $rankA <=> $rankB;
            }

            return $posA <=> $posB;
        });

        return array_map(fn (array $pair): array => $pair[0], $indexed);
    }

    private function processEntity(string $entityType, string $opType, array $data): void
    {
        switch ($entityType) {
            case 'Organization':
                $this->upsertOrganization($opType, $data);
                break;

            case 'AgeCategory':
                $this->upsertAgeCategory($opType, $data);
                break;

            case 'Beneficiary':
                $this->upsertBeneficiary($opType, $data);
                break;

            case 'EntryAttempt':
                $this->upsertEntryAttempt($opType, $data);
                break;

            case 'EntryAttemptCompanion':
                $this->upsertEntryAttemptCompanion($opType, $data);
                break;

            case 'GiftVoucher':
                $this->upsertGiftVoucher($opType, $data);
                break;

            default:
                Log::info("Ignoring unhandled sync entity type: {$entityType}");
                break;
        }
    }

    private function upsertOrganization(string $opType, array $data): void
    {
        $id = $data['Id'] ?? null;
        if (! $id) {
            return;
        }

        if ($opType === 'Delete') {
            Organization::destroy($id);

            return;
        }

        Organization::updateOrCreate(
            ['id' => $id],
            [
                'code' => $data['Code'] ?? '',
                'name_ar' => $data['NameAr'] ?? '',
                'cooldown_minutes' => $data['CooldownMinutes'] ?? null,
                'is_default' => (bool) ($data['IsDefault'] ?? false),
                'is_active' => (bool) ($data['IsActive'] ?? true),
                'max_free_companions' => $data['MaxFreeCompanions'] ?? null,
                'gift_enabled' => (bool) ($data['GiftEnabled'] ?? false),
                'gift_value' => $data['GiftValue'] ?? 0,
                'gift_interval_days' => $data['GiftIntervalDays'] ?? 30,
                'created_at_utc' => self::utc($data['CreatedAtUtc'] ?? null),
                'updated_at_utc' => self::utc($data['UpdatedAtUtc'] ?? null),
            ]
        );
    }

    private function upsertAgeCategory(string $opType, array $data): void
    {
        $id = $data['Id'] ?? null;
        if (! $id) {
            return;
        }

        if ($opType === 'Delete') {
            AgeCategory::destroy($id);

            return;
        }

        AgeCategory::updateOrCreate(
            ['id' => $id],
            [
                'name_ar' => $data['NameAr'] ?? '',
                'description_ar' => $data['DescriptionAr'] ?? null,
                'sort_order' => $data['SortOrder'] ?? 0,
                'is_active' => (bool) ($data['IsActive'] ?? true),
                'is_companion_limit_exempt' => (bool) ($data['IsCompanionLimitExempt'] ?? false),
            ]
        );
    }

    private function upsertBeneficiary(string $opType, array $data): void
    {
        $id = $data['Id'] ?? null;
        if (! $id) {
            return;
        }

        if ($opType === 'Delete') {
            Beneficiary::destroy($id);

            return;
        }

        // Contact and ID-number fields are deliberately never read (ADR-021): they stay in the park.
        Beneficiary::updateOrCreate(
            ['id' => $id],
            [
                'organization_id' => $data['OrganizationId'] ?? null,
                'card_number' => $data['CardNumber'] ?? '',
                'full_name' => $data['FullName'] ?? '',
                'card_expiry_date' => $data['CardExpiryDate'] ?? null,
                'status' => $data['Status'] ?? 1,
                'status_reason' => $data['StatusReason'] ?? null,
                'notes' => $data['Notes'] ?? null,
                'last_granted_entry_utc' => self::utc($data['LastGrantedEntryUtc'] ?? null),
                'created_at_utc' => self::utc($data['CreatedAtUtc'] ?? null),
                'updated_at_utc' => self::utc($data['UpdatedAtUtc'] ?? null),
            ]
        );
    }

    private function upsertEntryAttempt(string $opType, array $data): void
    {
        $id = $data['Id'] ?? null;
        if (! $id) {
            return;
        }

        if ($opType === 'Delete') {
            EntryAttempt::destroy($id);

            return;
        }

        EntryAttempt::updateOrCreate(
            ['id' => $id],
            [
                'beneficiary_id' => $data['BeneficiaryId'] ?? null,
                'organization_id' => $data['OrganizationId'] ?? null,
                'attempted_at_utc' => self::utc($data['AttemptedAtUtc'] ?? null) ?? now(),
                'business_date' => $data['BusinessDate'] ?? now()->toDateString(),
                'result' => $data['Result'] ?? 1,
                'denial_reason' => $data['DenialReason'] ?? null,
                'cooldown_minutes_applied' => $data['CooldownMinutesApplied'] ?? 0,
                'next_eligible_at_utc' => self::utc($data['NextEligibleAtUtc'] ?? null),
                'total_persons' => $data['TotalPersons'] ?? 1,
                'channel' => $data['Channel'] ?? 1,
                'workstation_id' => $data['WorkstationId'] ?? null,
                'user_id' => $data['UserId'] ?? null,
                'override_by_user_id' => $data['OverrideByUserId'] ?? null,
                'override_reason' => $data['OverrideReason'] ?? null,
                'is_voided' => (bool) ($data['IsVoided'] ?? false),
                'voided_at_utc' => self::utc($data['VoidedAtUtc'] ?? null),
                'voided_by_user_id' => $data['VoidedByUserId'] ?? null,
                'void_reason' => $data['VoidReason'] ?? null,
                'created_at_utc' => self::utc($data['CreatedAtUtc'] ?? null),
            ]
        );
    }

    private function upsertEntryAttemptCompanion(string $opType, array $data): void
    {
        $attemptId = $data['EntryAttemptId'] ?? null;
        $categoryId = $data['AgeCategoryId'] ?? null;
        if (! $attemptId || ! $categoryId) {
            return;
        }

        if ($opType === 'Delete') {
            EntryAttemptCompanion::where('entry_attempt_id', $attemptId)
                ->where('age_category_id', $categoryId)
                ->delete();

            return;
        }

        EntryAttemptCompanion::updateOrCreate(
            [
                'entry_attempt_id' => $attemptId,
                'age_category_id' => $categoryId,
            ],
            [
                'count' => $data['Count'] ?? 0,
            ]
        );
    }

    private function upsertGiftVoucher(string $opType, array $data): void
    {
        $id = $data['Id'] ?? null;
        if (! $id) {
            return;
        }

        if ($opType === 'Delete') {
            GiftVoucher::destroy($id);

            return;
        }

        // The voucher token and signing key id are deliberately never read (R2.7): the cloud only reports.
        GiftVoucher::updateOrCreate(
            ['id' => $id],
            [
                'beneficiary_id' => $data['BeneficiaryId'] ?? null,
                'organization_id' => $data['OrganizationId'] ?? null,
                'value' => $data['Value'] ?? 0,
                'issued_at_utc' => self::utc($data['IssuedAtUtc'] ?? null),
                'business_date' => $data['BusinessDate'] ?? now()->toDateString(),
                'expires_on' => $data['ExpiresOn'] ?? ($data['BusinessDate'] ?? now()->toDateString()),
                'status' => $data['Status'] ?? 1,
                'issued_by_user_id' => $data['IssuedByUserId'] ?? null,
                'issued_workstation_id' => $data['IssuedWorkstationId'] ?? null,
                'is_override' => (bool) ($data['IsOverride'] ?? false),
                'override_by_user_id' => $data['OverrideByUserId'] ?? null,
                'override_reason' => $data['OverrideReason'] ?? null,
                'redeemed_at_utc' => self::utc($data['RedeemedAtUtc'] ?? null),
                'redeemed_business_date' => $data['RedeemedBusinessDate'] ?? null,
                'redeemed_by_user_id' => $data['RedeemedByUserId'] ?? null,
                'redeemed_workstation_id' => $data['RedeemedWorkstationId'] ?? null,
                'voided_at_utc' => self::utc($data['VoidedAtUtc'] ?? null),
                'voided_by_user_id' => $data['VoidedByUserId'] ?? null,
                'void_reason' => $data['VoidReason'] ?? null,
            ]
        );
    }

    /**
     * Normalizes an ISO-8601 instant from the agent ("...T10:15:30.1234567Z" or with an offset)
     * to UTC, so MySQL strict mode accepts it and an offset is never read as UTC wall time (R2.9).
     * An unparseable value throws, which rejects the batch like any other malformed item.
     */
    private static function utc(mixed $value): ?CarbonImmutable
    {
        if ($value === null || $value === '') {
            return null;
        }

        return CarbonImmutable::parse((string) $value)->utc();
    }
}
