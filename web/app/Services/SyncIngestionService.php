<?php

namespace App\Services;

use App\Models\AgeCategory;
use App\Models\Beneficiary;
use App\Models\EntryAttempt;
use App\Models\EntryAttemptCompanion;
use App\Models\Organization;
use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Log;

class SyncIngestionService
{
    /**
     * Decrypt AES-256-CBC encrypted payload value.
     * C# payload format: Base64(16-byte IV + CipherText).
     */
    public static function decrypt(?string $cipherTextBase64, ?string $base64Key): ?string
    {
        if (empty($cipherTextBase64) || empty($base64Key)) {
            return $cipherTextBase64;
        }

        try {
            $raw = base64_decode($cipherTextBase64, true);
            $key = base64_decode($base64Key, true);

            if ($raw === false || $key === false || strlen($key) !== 32 || strlen($raw) < 16) {
                return $cipherTextBase64;
            }

            $iv = substr($raw, 0, 16);
            $cipher = substr($raw, 16);

            $decrypted = openssl_decrypt($cipher, 'AES-256-CBC', $key, OPENSSL_RAW_DATA, $iv);
            return $decrypted !== false ? $decrypted : $cipherTextBase64;
        } catch (\Throwable $e) {
            Log::warning("Decryption exception: {$e->getMessage()}");
            return $cipherTextBase64;
        }
    }

    /**
     * Ingests a batch of outbox records sent by ParkGate.SyncAgent.
     *
     * @param array $records
     * @return array
     */
    public function ingestBatch(array $records): array
    {
        $aesKey = config('parkgate.aes_key');
        $processed = 0;
        $failed = 0;

        DB::transaction(function () use ($records, $aesKey, &$processed, &$failed) {
            foreach ($records as $item) {
                try {
                    $entityType = $item['entityType'] ?? null;
                    $opType = $item['operationType'] ?? 'Insert';
                    $payloadRaw = $item['payloadJson'] ?? null;

                    if (empty($entityType) || empty($payloadRaw)) {
                        continue;
                    }

                    $data = json_decode($payloadRaw, true);
                    if (!is_array($data)) {
                        continue;
                    }

                    $this->processEntity($entityType, $opType, $data, $aesKey);
                    $processed++;
                } catch (\Throwable $e) {
                    $failed++;
                    Log::error("Failed to ingest sync item: {$e->getMessage()}", ['item' => $item]);
                    throw $e; // rollback transaction on batch failure
                }
            }
        });

        return [
            'processed' => $processed,
            'failed' => $failed,
        ];
    }

    private function processEntity(string $entityType, string $opType, array $data, ?string $aesKey): void
    {
        switch ($entityType) {
            case 'Organization':
                $this->upsertOrganization($opType, $data);
                break;

            case 'AgeCategory':
                $this->upsertAgeCategory($opType, $data);
                break;

            case 'Beneficiary':
                $this->upsertBeneficiary($opType, $data, $aesKey);
                break;

            case 'EntryAttempt':
                $this->upsertEntryAttempt($opType, $data);
                break;

            case 'EntryAttemptCompanion':
                $this->upsertEntryAttemptCompanion($opType, $data);
                break;

            default:
                Log::info("Ignoring unhandled sync entity type: {$entityType}");
                break;
        }
    }

    private function upsertOrganization(string $opType, array $data): void
    {
        $id = $data['Id'] ?? null;
        if (!$id) return;

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
                'is_default' => (bool)($data['IsDefault'] ?? false),
                'is_active' => (bool)($data['IsActive'] ?? true),
                'created_at_utc' => $data['CreatedAtUtc'] ?? null,
                'updated_at_utc' => $data['UpdatedAtUtc'] ?? null,
            ]
        );
    }

    private function upsertAgeCategory(string $opType, array $data): void
    {
        $id = $data['Id'] ?? null;
        if (!$id) return;

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
                'is_active' => (bool)($data['IsActive'] ?? true),
            ]
        );
    }

    private function upsertBeneficiary(string $opType, array $data, ?string $aesKey): void
    {
        $id = $data['Id'] ?? null;
        if (!$id) return;

        if ($opType === 'Delete') {
            Beneficiary::destroy($id);
            return;
        }

        $phone = $data['Phone'] ?? null;
        if ($phone && $aesKey) {
            $phone = self::decrypt($phone, $aesKey);
        }

        $nationalId = $data['NationalId'] ?? null;
        if ($nationalId && $aesKey) {
            $nationalId = self::decrypt($nationalId, $aesKey);
        }

        Beneficiary::updateOrCreate(
            ['id' => $id],
            [
                'organization_id' => $data['OrganizationId'] ?? null,
                'card_number' => $data['CardNumber'] ?? '',
                'full_name' => $data['FullName'] ?? '',
                'phone' => $phone,
                'national_id' => $nationalId,
                'card_expiry_date' => $data['CardExpiryDate'] ?? null,
                'status' => $data['Status'] ?? 1,
                'status_reason' => $data['StatusReason'] ?? null,
                'notes' => $data['Notes'] ?? null,
                'last_granted_entry_utc' => $data['LastGrantedEntryUtc'] ?? null,
                'created_at_utc' => $data['CreatedAtUtc'] ?? null,
                'updated_at_utc' => $data['UpdatedAtUtc'] ?? null,
            ]
        );
    }

    private function upsertEntryAttempt(string $opType, array $data): void
    {
        $id = $data['Id'] ?? null;
        if (!$id) return;

        if ($opType === 'Delete') {
            EntryAttempt::destroy($id);
            return;
        }

        EntryAttempt::updateOrCreate(
            ['id' => $id],
            [
                'beneficiary_id' => $data['BeneficiaryId'] ?? null,
                'organization_id' => $data['OrganizationId'] ?? null,
                'attempted_at_utc' => $data['AttemptedAtUtc'] ?? now(),
                'business_date' => $data['BusinessDate'] ?? now()->toDateString(),
                'result' => $data['Result'] ?? 1,
                'denial_reason' => $data['DenialReason'] ?? null,
                'cooldown_minutes_applied' => $data['CooldownMinutesApplied'] ?? 0,
                'next_eligible_at_utc' => $data['NextEligibleAtUtc'] ?? null,
                'total_persons' => $data['TotalPersons'] ?? 1,
                'channel' => $data['Channel'] ?? 1,
                'workstation_id' => $data['WorkstationId'] ?? null,
                'user_id' => $data['UserId'] ?? null,
                'override_by_user_id' => $data['OverrideByUserId'] ?? null,
                'override_reason' => $data['OverrideReason'] ?? null,
                'is_voided' => (bool)($data['IsVoided'] ?? false),
                'voided_at_utc' => $data['VoidedAtUtc'] ?? null,
                'voided_by_user_id' => $data['VoidedByUserId'] ?? null,
                'void_reason' => $data['VoidReason'] ?? null,
                'created_at_utc' => $data['CreatedAtUtc'] ?? null,
            ]
        );
    }

    private function upsertEntryAttemptCompanion(string $opType, array $data): void
    {
        $attemptId = $data['EntryAttemptId'] ?? null;
        $categoryId = $data['AgeCategoryId'] ?? null;
        if (!$attemptId || !$categoryId) return;

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
}
