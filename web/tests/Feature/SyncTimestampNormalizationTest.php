<?php

namespace Tests\Feature;

use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\DB;
use Tests\TestCase;

/**
 * R2.9: the agent serializes UTC instants as ISO-8601 with seven fractional digits and a "Z".
 * MySQL in strict mode rejects that shape in a timestamp column, and a payload carrying an
 * offset must not be stored as if its wall time were UTC. Ingestion normalizes every instant
 * to "Y-m-d H:i:s" in UTC.
 */
class SyncTimestampNormalizationTest extends TestCase
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

    public function test_seven_digit_utc_timestamps_are_stored_as_plain_utc(): void
    {
        $this->push([
            $this->record('Beneficiary', self::HOLDER, [
                'Id' => self::HOLDER, 'CardNumber' => 'C-1', 'FullName' => 'x', 'Status' => 1,
                'LastGrantedEntryUtc' => '2026-11-13T10:15:30.1234567Z',
                'CreatedAtUtc' => '2026-11-12T08:00:00.0000000Z',
            ]),
        ]);

        $row = DB::table('beneficiaries')->where('id', self::HOLDER)->first();

        $this->assertSame('2026-11-13 10:15:30', $row->last_granted_entry_utc);
        $this->assertSame('2026-11-12 08:00:00', $row->created_at_utc);
    }

    public function test_timestamps_with_an_offset_are_converted_to_utc(): void
    {
        $this->push([
            $this->record('Beneficiary', self::HOLDER, [
                'Id' => self::HOLDER, 'CardNumber' => 'C-1', 'FullName' => 'x', 'Status' => 1,
            ]),
            $this->record('EntryAttempt', self::ATTEMPT, [
                'Id' => self::ATTEMPT, 'BeneficiaryId' => self::HOLDER, 'Result' => 2, 'TotalPersons' => 1,
                'AttemptedAtUtc' => '2026-11-13T12:00:00.5000000+02:00',
                'NextEligibleAtUtc' => '2026-11-13T23:59:59-03:00',
                'BusinessDate' => '2026-11-13',
            ]),
        ]);

        $row = DB::table('entry_attempts')->where('id', self::ATTEMPT)->first();

        $this->assertSame('2026-11-13 10:00:00', $row->attempted_at_utc);
        $this->assertSame('2026-11-14 02:59:59', $row->next_eligible_at_utc);
    }

    public function test_an_unparseable_timestamp_rejects_the_batch(): void
    {
        $this->withHeaders(['X-Api-Key' => self::API_KEY])
            ->postJson('/api/sync/push', ['records' => [
                $this->record('Beneficiary', self::HOLDER, [
                    'Id' => self::HOLDER, 'CardNumber' => 'C-1', 'FullName' => 'x', 'Status' => 1,
                    'LastGrantedEntryUtc' => 'not a date',
                ]),
            ]])
            ->assertStatus(422)
            ->assertJson(['success' => false]);

        $this->assertDatabaseMissing('beneficiaries', ['id' => self::HOLDER]);
    }

    /**
     * @param  array<int, array<string, mixed>>  $records
     */
    private function push(array $records): void
    {
        $this->withHeaders(['X-Api-Key' => self::API_KEY])
            ->postJson('/api/sync/push', ['records' => $records])
            ->assertOk();
    }

    /**
     * @param  array<string, mixed>  $payload
     * @return array<string, mixed>
     */
    private function record(string $type, string $id, array $payload): array
    {
        return [
            'entityType' => $type,
            'entityId' => $id,
            'operationType' => 'Insert',
            'payloadJson' => json_encode($payload),
        ];
    }
}
