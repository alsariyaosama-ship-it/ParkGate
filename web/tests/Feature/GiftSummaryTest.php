<?php

namespace Tests\Feature;

use App\Models\Beneficiary;
use App\Models\GiftVoucher;
use App\Models\Organization;
use App\Models\User;
use Carbon\Carbon;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Tests\TestCase;

/**
 * R2.7: the dashboard and the reports summarize gift vouchers (issued, redeemed, voided, value) by organization.
 */
class GiftSummaryTest extends TestCase
{
    use RefreshDatabase;

    protected function setUp(): void
    {
        parent::setUp();
        Carbon::setTestNow(Carbon::parse('2026-11-13 12:00:00', 'Africa/Tripoli')->utc());

        Organization::create(['id' => 1, 'code' => 'SSF', 'name_ar' => 'التضامن', 'is_active' => true]);
        Organization::create(['id' => 2, 'code' => 'SHA', 'name_ar' => 'الشهداء', 'is_active' => true]);
        Beneficiary::create(['id' => '11111111-1111-1111-1111-111111111111', 'organization_id' => 1,
            'card_number' => 'C-1', 'full_name' => 'علي', 'status' => 1]);

        $this->voucher(1, org: 1, status: 1, value: 10);
        $this->voucher(2, org: 1, status: 2, value: 10);
        $this->voucher(3, org: 2, status: 2, value: 5.5);
        $this->voucher(4, org: 2, status: 3, value: 5.5);
        $this->voucher(5, org: 1, status: 2, value: 99, date: '2026-11-12');
    }

    protected function tearDown(): void
    {
        Carbon::setTestNow();
        parent::tearDown();
    }

    public function test_dashboard_summarizes_todays_vouchers(): void
    {
        $gifts = $this->props('/dashboard')['gifts'];

        $this->assertSame(4, $gifts['totals']['issued']);
        $this->assertSame(2, $gifts['totals']['redeemed']);
        $this->assertSame(1, $gifts['totals']['voided']);
        $this->assertSame(1, $gifts['totals']['outstanding']);
        $this->assertEqualsWithDelta(31.0, $gifts['totals']['value_issued'], 0.001);
        $this->assertEqualsWithDelta(15.5, $gifts['totals']['value_redeemed'], 0.001);
        $this->assertEqualsWithDelta(5.5, $gifts['totals']['value_voided'], 0.001);
    }

    public function test_dashboard_breaks_vouchers_down_by_organization(): void
    {
        $gifts = $this->props('/dashboard')['gifts'];

        $sha = collect($gifts['by_organization'])->firstWhere('organization_id', 2);
        $this->assertSame('الشهداء', $sha['name_ar']);
        $this->assertSame(2, $sha['issued']);
        $this->assertSame(1, $sha['redeemed']);
        $this->assertSame(1, $sha['voided']);
        $this->assertEqualsWithDelta(5.5, $sha['value_redeemed'], 0.001);
    }

    public function test_report_summarizes_the_selected_range_and_organization(): void
    {
        $props = $this->props('/reports?start_date=2026-11-12&end_date=2026-11-13&organization_id=1');

        $totals = $props['gifts']['totals'];
        $this->assertSame(3, $totals['issued']);
        $this->assertSame(2, $totals['redeemed']);
        $this->assertSame(1, $totals['outstanding']);
        $this->assertEqualsWithDelta(119.0, $totals['value_issued'], 0.001);
        $this->assertCount(1, $props['gifts']['by_organization']);
    }

    public function test_report_without_vouchers_returns_zero_totals(): void
    {
        $totals = $this->props('/reports?start_date=2026-01-01&end_date=2026-01-02')['gifts']['totals'];

        $this->assertSame(0, $totals['issued']);
        $this->assertEqualsWithDelta(0.0, $totals['value_issued'], 0.001);
    }

    private function props(string $url): array
    {
        return $this->actingAs(User::factory()->create())->get($url)->assertOk()->viewData('page')['props'];
    }

    private function voucher(int $id, int $org, int $status, float $value, string $date = '2026-11-13'): void
    {
        GiftVoucher::create([
            'id' => $id,
            'beneficiary_id' => '11111111-1111-1111-1111-111111111111',
            'organization_id' => $org,
            'value' => $value,
            'issued_at_utc' => $date.' 09:00:00',
            'business_date' => $date,
            'expires_on' => $date,
            'status' => $status,
            'issued_by_user_id' => 1,
            'issued_workstation_id' => 1,
            'is_override' => false,
        ]);
    }
}
