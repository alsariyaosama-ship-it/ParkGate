<?php

namespace Tests\Feature;

use App\Models\AgeCategory;
use App\Models\GiftVoucher;
use App\Models\Organization;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Schema;
use Tests\TestCase;

/**
 * R2.7: gift vouchers, the organization gift policy and the companion-limit exemption reach the cloud.
 */
class GiftVoucherSyncTest extends TestCase
{
    use RefreshDatabase;

    private const API_KEY = 'test-sync-api-key';

    private const HOLDER = '99999999-9999-9999-9999-999999999999';

    protected function setUp(): void
    {
        parent::setUp();
        config(['parkgate.api_key' => self::API_KEY]);
    }

    public function test_voucher_listed_before_its_parents_is_ingested_and_later_updates_it(): void
    {
        $this->push([
            $this->record('GiftVoucher', '7', $this->voucherPayload()),
            $this->record('Beneficiary', self::HOLDER, [
                'Id' => self::HOLDER, 'OrganizationId' => 10, 'CardNumber' => 'C-1', 'FullName' => 'علي', 'Status' => 1,
            ]),
            $this->record('Organization', '10', ['Id' => 10, 'Code' => 'SSF', 'NameAr' => 'جهة', 'IsActive' => true]),
        ])->assertOk()->assertJson(['processed' => 3]);

        $voucher = GiftVoucher::findOrFail(7);
        $this->assertSame(self::HOLDER, $voucher->beneficiary_id);
        $this->assertSame(10, $voucher->organization_id);
        $this->assertEqualsWithDelta(12.5, (float) $voucher->value, 0.0001);
        $this->assertSame(1, $voucher->status);
        $this->assertSame('2026-11-13', $voucher->business_date->toDateString());

        $this->push([$this->record('GiftVoucher', '7', $this->voucherPayload([
            'Status' => 2,
            'RedeemedAtUtc' => '2026-11-13T11:00:00Z',
            'RedeemedBusinessDate' => '2026-11-13',
            'RedeemedByUserId' => 3,
            'RedeemedWorkstationId' => 2,
        ]), 'Update')])->assertOk();

        $voucher->refresh();
        $this->assertSame(2, $voucher->status);
        $this->assertSame(1, GiftVoucher::count());
        $this->assertNotNull($voucher->redeemed_at_utc);
    }

    public function test_void_details_are_stored(): void
    {
        $this->seedParents();

        $this->push([$this->record('GiftVoucher', '7', $this->voucherPayload([
            'Status' => 3, 'VoidedAtUtc' => '2026-11-13T12:00:00Z', 'VoidedByUserId' => 1, 'VoidReason' => 'خطأ في الإصدار',
        ]))])->assertOk();

        $voucher = GiftVoucher::findOrFail(7);
        $this->assertSame(3, $voucher->status);
        $this->assertSame('خطأ في الإصدار', $voucher->void_reason);
    }

    public function test_voucher_can_be_deleted(): void
    {
        $this->seedParents();
        $this->push([$this->record('GiftVoucher', '7', $this->voucherPayload())])->assertOk();

        $this->push([$this->record('GiftVoucher', '7', ['Id' => 7], 'Delete')])->assertOk();

        $this->assertSame(0, GiftVoucher::count());
    }

    public function test_the_redeemable_token_has_no_column_and_is_never_stored(): void
    {
        $this->assertFalse(Schema::hasColumn('gift_vouchers', 'token'));
        $this->assertFalse(Schema::hasColumn('gift_vouchers', 'key_id'));

        $this->seedParents();
        $this->push([$this->record('GiftVoucher', '7', $this->voucherPayload([
            'Token' => 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 'KeyId' => 1,
        ]))])->assertOk();

        $this->assertStringNotContainsString(
            'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
            json_encode(GiftVoucher::findOrFail(7)->getAttributes())
        );
    }

    public function test_organization_gift_policy_and_age_category_exemption_are_stored(): void
    {
        $this->push([
            $this->record('Organization', '10', [
                'Id' => 10, 'Code' => 'SSF', 'NameAr' => 'جهة', 'IsActive' => true,
                'MaxFreeCompanions' => 4, 'GiftEnabled' => true, 'GiftValue' => 12.5, 'GiftIntervalDays' => 14,
            ]),
            $this->record('AgeCategory', '3', [
                'Id' => 3, 'NameAr' => 'رضع', 'SortOrder' => 3, 'IsActive' => true, 'IsCompanionLimitExempt' => true,
            ]),
        ])->assertOk();

        $org = Organization::findOrFail(10);
        $this->assertSame(4, $org->max_free_companions);
        $this->assertTrue($org->gift_enabled);
        $this->assertEqualsWithDelta(12.5, (float) $org->gift_value, 0.0001);
        $this->assertSame(14, $org->gift_interval_days);
        $this->assertTrue(AgeCategory::findOrFail(3)->is_companion_limit_exempt);
    }

    public function test_organization_without_the_new_fields_gets_safe_defaults(): void
    {
        $this->push([$this->record('Organization', '10', ['Id' => 10, 'Code' => 'SSF', 'NameAr' => 'جهة'])])->assertOk();

        $org = Organization::findOrFail(10);
        $this->assertNull($org->max_free_companions);
        $this->assertFalse($org->gift_enabled);
        $this->assertSame(30, $org->gift_interval_days);
    }

    private function seedParents(): void
    {
        $this->push([
            $this->record('Organization', '10', ['Id' => 10, 'Code' => 'SSF', 'NameAr' => 'جهة', 'IsActive' => true]),
            $this->record('Beneficiary', self::HOLDER, [
                'Id' => self::HOLDER, 'OrganizationId' => 10, 'CardNumber' => 'C-1', 'FullName' => 'علي', 'Status' => 1,
            ]),
        ])->assertOk();
    }

    private function voucherPayload(array $overrides = []): array
    {
        return array_merge([
            'Id' => 7,
            'BeneficiaryId' => self::HOLDER,
            'OrganizationId' => 10,
            'Value' => 12.5,
            'IssuedAtUtc' => '2026-11-13T09:00:00Z',
            'BusinessDate' => '2026-11-13',
            'ExpiresOn' => '2026-11-13',
            'Status' => 1,
            'IssuedByUserId' => 1,
            'IssuedWorkstationId' => 1,
            'IsOverride' => false,
        ], $overrides);
    }

    private function push(array $records)
    {
        return $this->withHeaders(['X-Api-Key' => self::API_KEY])->postJson('/api/sync/push', ['records' => $records]);
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
