<?php

namespace Tests\Feature;

use App\Services\SyncIngestionService;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Log\Events\MessageLogged;
use Illuminate\Support\Facades\Event;
use Tests\TestCase;

/**
 * R1.6: the sync API returns no exception text, logs no payload content,
 * validates the batch shape and is rate-limited.
 */
class SyncApiHardeningTest extends TestCase
{
    use RefreshDatabase;

    private const API_KEY = 'test-sync-api-key';

    private const SECRET_NAME = 'سالم الحساس';

    private const SECRET_CARD = 'CARD-SECRET-77';

    /** @var list<MessageLogged> */
    private array $logged = [];

    protected function setUp(): void
    {
        parent::setUp();
        config(['parkgate.api_key' => self::API_KEY]);
        config(['parkgate.aes_key' => 'a2tra2tra2tra2tra2tra2tra2tra2tra2tra2tra2s=']);
        Event::listen(MessageLogged::class, fn (MessageLogged $e) => $this->logged[] = $e);
    }

    public function test_missing_records_returns_422_with_validation_errors(): void
    {
        $this->push(['nothing' => true])
            ->assertStatus(422)
            ->assertJson(['success' => false])
            ->assertJsonValidationErrors('records');
    }

    public function test_record_without_required_keys_returns_422(): void
    {
        $this->push(['records' => [['entityType' => 'Beneficiary']]])
            ->assertStatus(422)
            ->assertJsonValidationErrors(['records.0.entityId', 'records.0.operationType', 'records.0.payloadJson']);
    }

    public function test_unknown_operation_type_returns_422(): void
    {
        $record = $this->beneficiaryRecord('Phone', 'x');
        $record['operationType'] = 'Truncate';

        $this->push(['records' => [$record]])->assertJsonValidationErrors('records.0.operationType');
    }

    public function test_batch_larger_than_limit_returns_422(): void
    {
        $records = array_fill(0, 501, $this->beneficiaryRecord('Phone', null));

        $this->push(['records' => $records])->assertJsonValidationErrors('records');
    }

    public function test_rejected_item_returns_generic_422_without_exception_text_or_payload_in_log(): void
    {
        $response = $this->push(['records' => [$this->beneficiaryRecord('Phone', 'not-valid-base64-ciphertext-!!!')]]);

        $response->assertStatus(422)->assertJson(['success' => false]);
        $errorId = $response->json('error_id');
        $this->assertMatchesRegularExpression('/^[0-9a-f]{12}$/', (string) $errorId);
        $this->assertResponseHidesInternals($response->getContent());

        $this->assertLogHasNoPayload();
        $this->assertLogMentions($errorId, 'Beneficiary', '99999999-9999-9999-9999-999999999999');
        $this->assertDatabaseHas('sync_batches', ['status' => 'Failed', 'error_message' => "error_id={$errorId}"]);
    }

    public function test_server_error_returns_generic_500_without_exception_text(): void
    {
        $this->mock(SyncIngestionService::class)
            ->shouldReceive('ingestBatch')
            ->andThrow(new \RuntimeException('SQLSTATE[23000]: insert values ('.self::SECRET_NAME.', '.self::SECRET_CARD.')'));

        $response = $this->push(['records' => [$this->beneficiaryRecord('Phone', null)]]);

        $response->assertStatus(500)->assertJson(['success' => false]);
        $this->assertResponseHidesInternals($response->getContent());
        $this->assertLogHasNoPayload();
        $this->assertLogMentions($response->json('error_id'));
    }

    public function test_sync_endpoints_are_rate_limited(): void
    {
        for ($i = 0; $i < 60; $i++) {
            $this->withHeaders(['X-Api-Key' => self::API_KEY])->postJson('/api/sync/heartbeat')->assertOk();
        }

        $this->withHeaders(['X-Api-Key' => self::API_KEY])->postJson('/api/sync/heartbeat')->assertStatus(429);
    }

    private function push(array $body): \Illuminate\Testing\TestResponse
    {
        return $this->withHeaders(['X-Api-Key' => self::API_KEY])->postJson('/api/sync/push', $body);
    }

    private function beneficiaryRecord(string $field, ?string $value): array
    {
        return [
            'id' => 1,
            'entityType' => 'Beneficiary',
            'entityId' => '99999999-9999-9999-9999-999999999999',
            'operationType' => 'Insert',
            'payloadJson' => json_encode([
                'Id' => '99999999-9999-9999-9999-999999999999',
                'CardNumber' => self::SECRET_CARD,
                'FullName' => self::SECRET_NAME,
                $field => $value,
            ]),
        ];
    }

    private function assertResponseHidesInternals(string $body): void
    {
        foreach (['SQLSTATE', 'Decryption', 'AES', 'ciphertext', self::SECRET_CARD] as $needle) {
            $this->assertStringNotContainsString($needle, $body);
        }
        $this->assertStringNotContainsString(self::SECRET_NAME, json_decode($body, true)['message'] ?? '');
    }

    private function assertLogHasNoPayload(): void
    {
        $text = $this->loggedText();
        foreach ([self::SECRET_NAME, self::SECRET_CARD, 'payloadJson', 'SQLSTATE'] as $needle) {
            $this->assertStringNotContainsString($needle, $text);
        }
    }

    private function assertLogMentions(string ...$needles): void
    {
        $text = $this->loggedText();
        foreach ($needles as $needle) {
            $this->assertStringContainsString($needle, $text);
        }
    }

    private function loggedText(): string
    {
        return implode("\n", array_map(
            fn (MessageLogged $e) => $e->message.' '.json_encode($e->context, JSON_UNESCAPED_UNICODE),
            $this->logged,
        ));
    }
}
