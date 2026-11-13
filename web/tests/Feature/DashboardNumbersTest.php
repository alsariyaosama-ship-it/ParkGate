<?php

namespace Tests\Feature;

use App\Models\Beneficiary;
use App\Models\EntryAttempt;
use App\Models\Organization;
use App\Models\User;
use Carbon\Carbon;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

/**
 * R2.8: dashboard numbers follow the park's business day and count what really entered.
 */
class DashboardNumbersTest extends TestCase
{
    use RefreshDatabase;

    protected function tearDown(): void
    {
        Carbon::setTestNow();
        parent::tearDown();
    }

    public function test_entries_and_persons_count_allowed_and_overridden_but_not_voided(): void
    {
        $this->travelToParkTime('2026-11-13 12:00:00');
        $this->attempt('a1', result: 1, persons: 3);
        $this->attempt('a2', result: 3, persons: 2);
        $this->attempt('a3', result: 1, persons: 5, voided: true);
        $this->attempt('a4', result: 2, persons: 4);

        $kpis = $this->dashboard()['kpis'];

        $this->assertSame(2, $kpis['today_entries']);
        $this->assertSame(5, $kpis['today_persons']);
        $this->assertSame(1, $kpis['today_overridden']);
        $this->assertSame(1, $kpis['today_denied']);
    }

    public function test_today_is_the_park_business_date_not_the_utc_date(): void
    {
        // 23:30 UTC on the 13th is 01:30 on the 14th in Tripoli (UTC+2).
        Carbon::setTestNow(Carbon::parse('2026-11-13 23:30:00', 'UTC'));
        $this->attempt('late', result: 1, persons: 2, businessDate: '2026-11-14');
        $this->attempt('yesterday', result: 1, persons: 9, businessDate: '2026-11-13');

        $kpis = $this->dashboard()['kpis'];

        $this->assertSame(1, $kpis['today_entries']);
        $this->assertSame(2, $kpis['today_persons']);
    }

    public function test_organization_breakdown_uses_the_same_rules(): void
    {
        $this->travelToParkTime('2026-11-13 12:00:00');
        $this->attempt('a1', result: 1, persons: 3);
        $this->attempt('a2', result: 3, persons: 2);
        $this->attempt('a3', result: 1, persons: 5, voided: true);
        $this->attempt('a4', result: 2, persons: 4);

        $org = collect($this->dashboard()['org_breakdown'])->firstWhere('id', 1);

        $this->assertSame(2, $org['today_entries']);
        $this->assertSame(5, $org['today_persons']);
    }

    public function test_hourly_buckets_use_park_local_time(): void
    {
        $this->travelToParkTime('2026-11-13 20:00:00');
        // 10:00 UTC is 12:00 in Tripoli.
        $this->attempt('a1', result: 1, persons: 4, attemptedAtUtc: '2026-11-13 10:00:00');

        $hourly = $this->dashboard()['hourly_stats'];

        $this->assertSame(4, $hourly['12:00']);
        $this->assertSame(0, $hourly['10:00']);
    }

    private function travelToParkTime(string $localTime): void
    {
        Carbon::setTestNow(Carbon::parse($localTime, 'Africa/Tripoli')->utc());
    }

    private function dashboard(): array
    {
        return $this->actingAs(User::factory()->create())->get('/dashboard')->assertOk()->viewData('page')['props'];
    }

    private function attempt(
        string $key,
        int $result,
        int $persons,
        bool $voided = false,
        ?string $businessDate = null,
        string $attemptedAtUtc = '2026-11-13 10:00:00',
    ): void {
        Organization::firstOrCreate(['id' => 1], ['code' => 'SSF', 'name_ar' => 'جهة', 'is_active' => true]);
        Beneficiary::firstOrCreate(['id' => '11111111-1111-1111-1111-111111111111'], [
            'organization_id' => 1, 'card_number' => 'C-1', 'full_name' => 'علي', 'status' => 1,
        ]);

        EntryAttempt::create([
            'id' => str_pad($key, 8, '0').'-0000-0000-0000-000000000000',
            'beneficiary_id' => '11111111-1111-1111-1111-111111111111',
            'organization_id' => 1,
            'attempted_at_utc' => $attemptedAtUtc,
            'business_date' => $businessDate ?? '2026-11-13',
            'result' => $result,
            'total_persons' => $persons,
            'channel' => 1,
            'is_voided' => $voided,
        ]);
    }
}
