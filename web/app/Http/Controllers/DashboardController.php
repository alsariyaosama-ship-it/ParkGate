<?php

namespace App\Http\Controllers;

use App\Models\Beneficiary;
use App\Models\EntryAttempt;
use App\Models\Organization;
use App\Models\ParkHeartbeat;
use App\Models\SyncBatch;
use Carbon\Carbon;
use Illuminate\Http\Request;
use Inertia\Inertia;
use Inertia\Response;

class DashboardController extends Controller
{
    /** Granted and supervisor-overridden attempts both let people in. */
    private const ENTRY_RESULTS = [1, 3];

    public function index(Request $request): Response
    {
        // business_date is the park's local date, so "today" is too (R2.8). The app timezone stays UTC
        // because every *_utc column is stored and read as UTC.
        $parkNow = Carbon::now(config('parkgate.timezone'));
        $today = $parkNow->toDateString();

        // 1. Park Connection Status
        $latestHeartbeat = ParkHeartbeat::latest('last_heartbeat_at')->first();
        $isOnline = false;
        $lastHeartbeatDiff = null;

        if ($latestHeartbeat) {
            $diffSeconds = Carbon::now()->diffInSeconds($latestHeartbeat->last_heartbeat_at);
            $isOnline = $diffSeconds <= 180; // considered online if heartbeat within 3 minutes
            $lastHeartbeatDiff = $latestHeartbeat->last_heartbeat_at->diffForHumans();
        }

        $latestSync = SyncBatch::latest('created_at')->first();

        // 2. Summary KPI Metrics. An entry is a granted (1) or overridden (3) attempt that was not voided.
        $entries = EntryAttempt::where('business_date', $today)
            ->whereIn('result', self::ENTRY_RESULTS)
            ->where('is_voided', false);

        $entryTotals = (clone $entries)
            ->selectRaw('COUNT(*) as entries, COALESCE(SUM(total_persons), 0) as persons')
            ->first();
        $todayEntries = (int) $entryTotals->entries;
        $todayPersons = (int) $entryTotals->persons;

        $todayOverridden = (clone $entries)->where('result', 3)->count();
        $todayDenied = EntryAttempt::where('business_date', $today)->where('result', 2)->count();

        $totalBeneficiaries = Beneficiary::count();
        $activeBeneficiaries = Beneficiary::where('status', 1)->count();

        // 3. Organization Breakdown (Today), aggregated in SQL
        $perOrganization = (clone $entries)
            ->selectRaw('organization_id, COUNT(*) as entries, COALESCE(SUM(total_persons), 0) as persons')
            ->groupBy('organization_id')
            ->get()
            ->keyBy('organization_id');

        $orgBreakdown = Organization::all()->map(fn ($org) => [
            'id' => $org->id,
            'name_ar' => $org->name_ar,
            'code' => $org->code,
            'today_entries' => (int) ($perOrganization[$org->id]->entries ?? 0),
            'today_persons' => (int) ($perOrganization[$org->id]->persons ?? 0),
        ]);

        // 4. Hourly Distribution (Today, park-local hours). Only two columns are read; converting hours
        // in SQL would need MySQL time-zone tables, which shared hosting often lacks.
        $hourlyData = [];
        for ($h = 8; $h <= 23; $h++) {
            $hourlyData[sprintf('%02d:00', $h)] = 0;
        }

        foreach ((clone $entries)->get(['attempted_at_utc', 'total_persons']) as $att) {
            if (! $att->attempted_at_utc) {
                continue;
            }

            $key = sprintf('%02d:00', $att->attempted_at_utc->copy()->setTimezone(config('parkgate.timezone'))->hour);
            if (isset($hourlyData[$key])) {
                $hourlyData[$key] += $att->total_persons;
            }
        }

        // 5. Recent 10 Synced Admissions
        $recentEntries = EntryAttempt::with(['beneficiary', 'organization'])
            ->latest('attempted_at_utc')
            ->take(10)
            ->get()
            ->map(function ($item) {
                return [
                    'id' => $item->id,
                    'beneficiary_name' => $item->beneficiary?->full_name ?? '—',
                    'card_number' => $item->beneficiary?->card_number ?? '—',
                    'organization_name' => $item->organization?->name_ar ?? '—',
                    'total_persons' => $item->total_persons,
                    'result' => $item->result,
                    'time_formatted' => $item->attempted_at_utc ? $item->attempted_at_utc->format('H:i') : '—',
                    'channel' => $item->channel,
                ];
            });

        return Inertia::render('Dashboard', [
            'status' => [
                'is_online' => $isOnline,
                'pending_count' => $latestHeartbeat?->pending_count ?? 0,
                'failed_count' => $latestHeartbeat?->failed_count ?? 0,
                'last_heartbeat' => $lastHeartbeatDiff,
                'last_sync' => $latestSync?->created_at?->diffForHumans() ?? '—',
            ],
            'kpis' => [
                'today_entries' => $todayEntries,
                'today_denied' => $todayDenied,
                'today_overridden' => $todayOverridden,
                'today_persons' => $todayPersons,
                'total_beneficiaries' => $totalBeneficiaries,
                'active_beneficiaries' => $activeBeneficiaries,
            ],
            'org_breakdown' => $orgBreakdown,
            'hourly_stats' => $hourlyData,
            'recent_entries' => $recentEntries,
        ]);
    }
}
