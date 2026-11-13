<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

/**
 * R2.4: a batch is ingested parent-first whatever order the agent sent it in,
 * so foreign keys hold inside the single batch transaction.
 */
class SyncIngestionOrderingTest extends TestCase
{
    use RefreshDatabase;

    private const API_KEY = 'test-sync-api-key';

    private const HOLDER = '99999999-9999-9999-9999-999999999999';

    private const ATTEMPT = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

    protected function setUp(): void
    {
        parent::setUp();
        config(['parkgate.api_key' => self::API_KEY]);
    }

    public function test_children_listed_before_parents_are_ingested(): void
    {
        $records = [
            $this->record('EntryAttemptCompanion', 'c1', ['EntryAttemptId' => self::ATTEMPT, 'AgeCategoryId' => 1, 'Count' => 2]),
            $this->record('EntryAttempt', self::ATTEMPT, [
                'Id' => self::ATTEMPT, 'BeneficiaryId' => self::HOLDER, 'OrganizationId' => 10,
                'AttemptedAtUtc' => '2026-11-13T10:00:00Z', 'BusinessDate' => '2026-11-13', 'Result' => 1, 'TotalPersons' => 3,
            ]),
            $this->record('Beneficiary', self::HOLDER, [
                'Id' => self::HOLDER, 'OrganizationId' => 10, 'CardNumber' => 'CARD-1', 'FullName' => 'محمد', 'Status' => 1,
            ]),
            $this->record('AgeCategory', '1', ['Id' => 1, 'NameAr' => 'أطفال', 'SortOrder' => 1, 'IsActive' => true]),
            $this->record('Organization', '10', ['Id' => 10, 'Code' => 'SSF', 'NameAr' => 'جهة', 'IsActive' => true]),
        ];

        $this->withHeaders(['X-Api-Key' => self::API_KEY])
            ->postJson('/api/sync/push', ['records' => $records])
            ->assertOk()
            ->assertJson(['success' => true, 'processed' => 5]);

        $this->assertDatabaseHas('beneficiaries', ['id' => self::HOLDER]);
        $this->assertDatabaseHas('entry_attempts', ['id' => self::ATTEMPT, 'beneficiary_id' => self::HOLDER]);
        $this->assertDatabaseHas('entry_attempt_companions', ['entry_attempt_id' => self::ATTEMPT, 'age_category_id' => 1, 'count' => 2]);
    }

    public function test_deletes_run_after_upserts_and_children_are_deleted_before_parents(): void
    {
        $this->withHeaders(['X-Api-Key' => self::API_KEY])->postJson('/api/sync/push', ['records' => [
            $this->record('Organization', '10', ['Id' => 10, 'Code' => 'SSF', 'NameAr' => 'جهة', 'IsActive' => true]),
            $this->record('AgeCategory', '1', ['Id' => 1, 'NameAr' => 'أطفال', 'SortOrder' => 1, 'IsActive' => true]),
            $this->record('Beneficiary', self::HOLDER, ['Id' => self::HOLDER, 'OrganizationId' => 10, 'CardNumber' => 'C', 'FullName' => 'x', 'Status' => 1]),
            $this->record('EntryAttempt', self::ATTEMPT, [
                'Id' => self::ATTEMPT, 'BeneficiaryId' => self::HOLDER, 'OrganizationId' => 10,
                'AttemptedAtUtc' => '2026-11-13T10:00:00Z', 'BusinessDate' => '2026-11-13', 'Result' => 1, 'TotalPersons' => 1,
            ]),
        ]])->assertOk();

        $this->withHeaders(['X-Api-Key' => self::API_KEY])->postJson('/api/sync/push', ['records' => [
            $this->record('Beneficiary', self::HOLDER, ['Id' => self::HOLDER], 'Delete'),
            $this->record('EntryAttempt', self::ATTEMPT, ['Id' => self::ATTEMPT], 'Delete'),
        ]])->assertOk()->assertJson(['processed' => 2]);

        $this->assertDatabaseMissing('beneficiaries', ['id' => self::HOLDER]);
        $this->assertDatabaseMissing('entry_attempts', ['id' => self::ATTEMPT]);
    }

    private function record(string $type, string $id, array $payload, string $op = 'Insert'): array
    {
        return [
            'entityType' => $type,
            'entityId' => $id,
            'operationType' => $op,
            'payloadJson' => json_encode($payload),
        ];
    }
}
