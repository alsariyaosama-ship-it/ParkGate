<?php

namespace Tests\Feature;

use App\Models\Beneficiary;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

class SyncApiTest extends TestCase
{
    use RefreshDatabase;

    protected string $apiKey = 'test-sync-api-key';

    protected function setUp(): void
    {
        parent::setUp();
        config(['parkgate.api_key' => $this->apiKey]);
    }

    public function test_push_requires_api_key(): void
    {
        $response = $this->postJson('/api/sync/push', ['records' => []]);
        $response->assertStatus(401);

        $responseInvalid = $this->withHeaders(['X-Api-Key' => 'wrong-key'])
            ->postJson('/api/sync/push', ['records' => []]);
        $responseInvalid->assertStatus(401);
    }

    public function test_heartbeat_records_pulse(): void
    {
        $response = $this->withHeaders(['X-Api-Key' => $this->apiKey])
            ->postJson('/api/sync/heartbeat');

        $response->assertStatus(200)
            ->assertJson(['success' => true, 'status' => 'online']);

        $this->assertDatabaseCount('park_heartbeats', 1);
    }

    public function test_push_ingests_organization_and_beneficiary(): void
    {
        $records = [
            [
                'id' => 1,
                'entityType' => 'Organization',
                'entityId' => '10',
                'operationType' => 'Insert',
                'payloadJson' => json_encode([
                    'Id' => 10,
                    'Code' => 'SSF',
                    'NameAr' => 'صندوق التضامن الاجتماعي',
                    'CooldownMinutes' => 720,
                    'IsDefault' => true,
                    'IsActive' => true,
                ]),
            ],
            [
                'id' => 2,
                'entityType' => 'AgeCategory',
                'entityId' => '1',
                'operationType' => 'Insert',
                'payloadJson' => json_encode([
                    'Id' => 1,
                    'NameAr' => 'أطفال',
                    'DescriptionAr' => 'من 3 إلى 12 سنة',
                    'SortOrder' => 1,
                    'IsActive' => true,
                ]),
            ],
            [
                'id' => 3,
                'entityType' => 'Beneficiary',
                'entityId' => '99999999-9999-9999-9999-999999999999',
                'operationType' => 'Insert',
                'payloadJson' => json_encode([
                    'Id' => '99999999-9999-9999-9999-999999999999',
                    'OrganizationId' => 10,
                    'CardNumber' => 'CARD-1234',
                    'FullName' => 'محمد أحمد علي',
                    'Status' => 1,
                ]),
            ],
            [
                'id' => 4,
                'entityType' => 'EntryAttempt',
                'entityId' => 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                'operationType' => 'Insert',
                'payloadJson' => json_encode([
                    'Id' => 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                    'BeneficiaryId' => '99999999-9999-9999-9999-999999999999',
                    'OrganizationId' => 10,
                    'AttemptedAtUtc' => now()->toISOString(),
                    'BusinessDate' => now()->toDateString(),
                    'Result' => 1, // Granted
                    'TotalPersons' => 3,
                    'Channel' => 1,
                ]),
            ],
        ];

        $response = $this->withHeaders(['X-Api-Key' => $this->apiKey])
            ->postJson('/api/sync/push', ['records' => $records]);

        $response->assertStatus(200)
            ->assertJson([
                'success' => true,
                'processed' => 4,
            ]);

        $this->assertDatabaseHas('organizations', [
            'id' => 10,
            'code' => 'SSF',
            'name_ar' => 'صندوق التضامن الاجتماعي',
        ]);

        $this->assertDatabaseHas('age_categories', [
            'id' => 1,
            'name_ar' => 'أطفال',
        ]);

        // Beneficiary should exist (phone and national ID are never stored: Q-R2)
        $this->assertDatabaseHas('beneficiaries', [
            'id' => '99999999-9999-9999-9999-999999999999',
            'card_number' => 'CARD-1234',
            'full_name' => 'محمد أحمد علي',
        ]);

        $this->assertDatabaseHas('entry_attempts', [
            'id' => 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            'total_persons' => 3,
            'result' => 1,
        ]);

        $this->assertDatabaseCount('sync_batches', 1);
    }

    public function test_push_with_non_object_payload_returns_422(): void
    {
        $records = [
            [
                'id' => 1,
                'entityType' => 'Beneficiary',
                'entityId' => '99999999-9999-9999-9999-999999999999',
                'operationType' => 'Insert',
                // Valid JSON, but not an object: the item is rejected (422), never silently skipped.
                'payloadJson' => json_encode('not an object'),
            ],
        ];

        $response = $this->withHeaders(['X-Api-Key' => $this->apiKey])
            ->postJson('/api/sync/push', ['records' => $records]);

        $response->assertStatus(422)
            ->assertJson(['success' => false]);
    }
}
