<?php

namespace App\Services;

use App\Models\GiftVoucher;
use App\Models\Organization;

/**
 * Gift voucher totals for the dashboard and the reports (R2.7).
 *
 * Vouchers are grouped by the business date they were issued on. A voucher expires on that same day,
 * so issue date and redeem date always agree. Status: 1 outstanding, 2 redeemed, 3 voided.
 */
class GiftSummaryService
{
    private const OUTSTANDING = 1;

    private const REDEEMED = 2;

    private const VOIDED = 3;

    /**
     * @return array{totals: array<string, int|float>, by_organization: array<int, array<string, mixed>>}
     */
    public function summarize(string $fromDate, string $toDate, ?int $organizationId = null): array
    {
        $query = GiftVoucher::query()->whereBetween('business_date', [$fromDate, $toDate]);

        if ($organizationId) {
            $query->where('organization_id', $organizationId);
        }

        $rows = $query
            ->selectRaw($this->aggregates().', organization_id')
            ->groupBy('organization_id')
            ->get();

        $names = Organization::whereIn('id', $rows->pluck('organization_id')->filter())->pluck('name_ar', 'id');

        $byOrganization = $rows
            ->map(fn ($row) => ['organization_id' => $row->organization_id, 'name_ar' => $names[$row->organization_id] ?? '—'] + $this->shape($row))
            ->sortBy('name_ar')
            ->values()
            ->all();

        return [
            'totals' => $this->sumTotals($byOrganization),
            'by_organization' => $byOrganization,
        ];
    }

    private function aggregates(): string
    {
        $count = fn (string $alias, ?int $status) => $status === null
            ? "COUNT(*) as {$alias}"
            : "COALESCE(SUM(CASE WHEN status = {$status} THEN 1 ELSE 0 END), 0) as {$alias}";
        $value = fn (string $alias, ?int $status) => $status === null
            ? "COALESCE(SUM(value), 0) as {$alias}"
            : "COALESCE(SUM(CASE WHEN status = {$status} THEN value ELSE 0 END), 0) as {$alias}";

        return implode(', ', [
            $count('issued', null),
            $count('redeemed', self::REDEEMED),
            $count('voided', self::VOIDED),
            $count('outstanding', self::OUTSTANDING),
            $value('value_issued', null),
            $value('value_redeemed', self::REDEEMED),
            $value('value_voided', self::VOIDED),
            $value('value_outstanding', self::OUTSTANDING),
        ]);
    }

    /** @return array<string, int|float> */
    private function shape(object $row): array
    {
        return [
            'issued' => (int) $row->issued,
            'redeemed' => (int) $row->redeemed,
            'voided' => (int) $row->voided,
            'outstanding' => (int) $row->outstanding,
            'value_issued' => (float) $row->value_issued,
            'value_redeemed' => (float) $row->value_redeemed,
            'value_voided' => (float) $row->value_voided,
            'value_outstanding' => (float) $row->value_outstanding,
        ];
    }

    /**
     * @param  array<int, array<string, mixed>>  $rows
     * @return array<string, int|float>
     */
    private function sumTotals(array $rows): array
    {
        $totals = $this->shape((object) array_fill_keys(
            ['issued', 'redeemed', 'voided', 'outstanding', 'value_issued', 'value_redeemed', 'value_voided', 'value_outstanding'],
            0,
        ));

        foreach ($rows as $row) {
            foreach (array_keys($totals) as $key) {
                $totals[$key] += $row[$key];
            }
        }

        return $totals;
    }
}
