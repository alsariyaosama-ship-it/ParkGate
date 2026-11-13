<?php

namespace Tests\Feature;

use App\Models\Beneficiary;
use App\Models\Organization;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Schema;
use Tests\TestCase;

/**
 * R2.6 (Q-R2): phone numbers and national IDs never reach the cloud database.
 */
class SensitiveFieldsTest extends TestCase
{
    use RefreshDatabase;

    private const API_KEY = 'test-sync-api-key';

    private const HOLDER_ID = '99999999-9999-9999-9999-999999999999';

    protected function setUp(): void
    {
        parent::setUp();
        config(['parkgate.api_key' => self::API_KEY]);
    }

    public function test_beneficiaries_table_has_no_phone_or_national_id_column(): void
    {
        $this->assertFalse(Schema::hasColumn('beneficiaries', 'phone'));
        $this->assertFalse(Schema::hasColumn('beneficiaries', 'national_id'));
    }

    public function test_beneficiary_model_does_not_accept_phone_or_national_id(): void
    {
        $fillable = (new Beneficiary())->getFillable();

        $this->assertNotContains('phone', $fillable);
        $this->assertNotContains('national_id', $fillable);
    }

    public function test_payload_without_sensitive_fields_ingests_and_keeps_notes_and_status_reason(): void
    {
        Organization::create(['id' => 10, 'code' => 'SSF', 'name_ar' => 'جهة', 'is_active' => true]);

        $this->push([
            'Id' => self::HOLDER_ID,
            'OrganizationId' => 10,
            'CardNumber' => 'CARD-1',
            'FullName' => 'محمد أحمد',
            'Status' => 2,
            'StatusReason' => 'مراجعة',
            'Notes' => 'ملاحظة',
        ])->assertOk()->assertJson(['success' => true, 'processed' => 1]);

        $this->assertDatabaseHas('beneficiaries', [
            'id' => self::HOLDER_ID,
            'status_reason' => 'مراجعة',
            'notes' => 'ملاحظة',
        ]);
    }

    public function test_payload_that_still_carries_sensitive_fields_is_ingested_without_them(): void
    {
        $this->push([
            'Id' => self::HOLDER_ID,
            'CardNumber' => 'CARD-1',
            'FullName' => 'محمد أحمد',
            'Phone' => '0912345678',
            'NationalId' => '119901234567',
        ])->assertOk();

        $stored = json_encode(\DB::table('beneficiaries')->where('id', self::HOLDER_ID)->first());
        $this->assertStringNotContainsString('0912345678', $stored);
        $this->assertStringNotContainsString('119901234567', $stored);
    }

    public function test_aes_key_is_no_longer_configured(): void
    {
        $this->assertArrayNotHasKey('aes_key', config('parkgate'));
    }

    private function push(array $payload): \Illuminate\Testing\TestResponse
    {
        return $this->withHeaders(['X-Api-Key' => self::API_KEY])->postJson('/api/sync/push', [
            'records' => [[
                'id' => 1,
                'entityType' => 'Beneficiary',
                'entityId' => self::HOLDER_ID,
                'operationType' => 'Insert',
                'payloadJson' => json_encode($payload),
            ]],
        ]);
    }
}
