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
 * R7.5: every time the web shows is park-local (Africa/Tripoli, UTC+2), dd/MM/yyyy and hh:mm with ص/م.
 */
class ParkTimeDisplayTest extends TestCase
{
    use RefreshDatabase;

    protected function tearDown(): void
    {
        Carbon::setTestNow();
        parent::tearDown();
    }

    public function test_dashboard_recent_entries_show_park_local_time_with_am_pm(): void
    {
        Carbon::setTestNow(Carbon::parse('2026-11-13 18:00:00', 'UTC'));
        $this->attempt('morning', '2026-11-13 08:30:00');
        $this->attempt('afternoon', '2026-11-13 13:05:00');

        $times = collect($this->props('/dashboard')['recent_entries'])->pluck('time_formatted')->all();

        $this->assertSame(['03:05 م', '10:30 ص'], $times);
    }

    public function test_report_rows_show_park_local_date_and_time(): void
    {
        $this->attempt('late', '2026-11-13 22:15:00', businessDate: '2026-11-14');

        $row = $this->props('/reports?start_date=2026-11-14&end_date=2026-11-14')['attempts']['data'][0];

        $this->assertSame('14/11/2026', $row['date_formatted']);
        $this->assertSame('12:15 ص', $row['time_formatted']);
    }

    public function test_beneficiary_last_entry_is_park_local(): void
    {
        $this->attempt('any', '2026-11-13 08:30:00');
        Beneficiary::query()->update(['last_granted_entry_utc' => '2026-11-13 08:30:00']);

        $row = $this->props('/beneficiaries')['beneficiaries']['data'][0];

        $this->assertSame('13/11/2026 10:30 ص', $row['last_entry']);
    }

    /**
     * @return array<string, mixed>
     */
    private function props(string $url): array
    {
        return $this->actingAs(User::factory()->create())->get($url)->assertOk()->viewData('page')['props'];
    }

    private function attempt(string $key, string $attemptedAtUtc, string $businessDate = '2026-11-13'): void
    {
        Organization::firstOrCreate(['id' => 1], ['code' => 'SSF', 'name_ar' => 'جهة', 'is_active' => true]);
        Beneficiary::firstOrCreate(['id' => '11111111-1111-1111-1111-111111111111'], [
            'organization_id' => 1, 'card_number' => 'C-1', 'full_name' => 'علي', 'status' => 1,
        ]);

        EntryAttempt::create([
            'id' => substr(md5($key), 0, 8).'-0000-0000-0000-000000000000',
            'beneficiary_id' => '11111111-1111-1111-1111-111111111111',
            'organization_id' => 1,
            'attempted_at_utc' => $attemptedAtUtc,
            'business_date' => $businessDate,
            'result' => 1,
            'total_persons' => 2,
            'channel' => 1,
            'is_voided' => false,
        ]);
    }
}
