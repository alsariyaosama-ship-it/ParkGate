<?php

namespace Tests\Feature;

use App\Models\ParkHeartbeat;
use App\Models\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Testing\TestResponse;
use PHPUnit\Framework\Attributes\DataProvider;
use Tests\TestCase;

/**
 * R2.5: the agent reports how many outbox rows are pending and how many are stuck,
 * and the dashboard shows it.
 */
class SyncHeartbeatTest extends TestCase
{
    use RefreshDatabase;

    private const API_KEY = 'test-sync-api-key';

    protected function setUp(): void
    {
        parent::setUp();
        config(['parkgate.api_key' => self::API_KEY]);
    }

    public function test_heartbeat_stores_pending_and_failed_counts(): void
    {
        $this->heartbeat(['pendingCount' => 7, 'failedCount' => 2])
            ->assertOk()
            ->assertJson(['success' => true, 'status' => 'online']);

        $this->assertDatabaseHas('park_heartbeats', ['pending_count' => 7, 'failed_count' => 2]);
    }

    public function test_heartbeat_without_counts_is_still_accepted_and_stores_zeros(): void
    {
        $this->heartbeat([])->assertOk();

        $this->assertDatabaseHas('park_heartbeats', ['pending_count' => 0, 'failed_count' => 0]);
    }

    #[DataProvider('invalidCounts')]
    public function test_heartbeat_rejects_invalid_counts(array $body): void
    {
        $this->heartbeat($body)->assertStatus(422);

        $this->assertDatabaseCount('park_heartbeats', 0);
    }

    public static function invalidCounts(): array
    {
        return [
            'negative pending' => [['pendingCount' => -1]],
            'text failed' => [['failedCount' => 'many']],
            'huge failed' => [['failedCount' => 99999999999]],
        ];
    }

    public function test_dashboard_shows_the_latest_heartbeat_counts(): void
    {
        ParkHeartbeat::create(['last_heartbeat_at' => now()->subMinutes(10), 'pending_count' => 1, 'failed_count' => 1, 'created_at' => now()]);
        ParkHeartbeat::create(['last_heartbeat_at' => now(), 'pending_count' => 7, 'failed_count' => 3, 'created_at' => now()]);

        $response = $this->actingAs(User::factory()->create())->get('/dashboard');

        $status = $response->viewData('page')['props']['status'];
        $this->assertSame(7, $status['pending_count']);
        $this->assertSame(3, $status['failed_count']);
        $this->assertTrue($status['is_online']);
    }

    public function test_dashboard_without_any_heartbeat_reports_zero_counts(): void
    {
        $response = $this->actingAs(User::factory()->create())->get('/dashboard');

        $status = $response->viewData('page')['props']['status'];
        $this->assertSame(0, $status['pending_count']);
        $this->assertSame(0, $status['failed_count']);
        $this->assertFalse($status['is_online']);
    }

    private function heartbeat(array $body): TestResponse
    {
        return $this->withHeaders(['X-Api-Key' => self::API_KEY])->postJson('/api/sync/heartbeat', $body);
    }
}
