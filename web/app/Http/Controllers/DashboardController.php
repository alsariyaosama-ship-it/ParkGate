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
    public function index(Request $request): Response
    {
        $today = Carbon::today()->toDateString();

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

        // 2. Summary KPI Metrics
        $todayAttempts = EntryAttempt::where('business_date', $today)->get();

        $todayAllowed = $todayAttempts->where('result', 1)->count();
        $todayDenied = $todayAttempts->where('result', 2)->count();
        $todayOverridden = $todayAttempts->where('result', 3)->count();
        $todayPersons = $todayAttempts->where('result', 1)->sum('total_persons');

        $totalBeneficiaries = Beneficiary::count();
        $activeBeneficiaries = Beneficiary::where('status', 1)->count();

        // 3. Organization Breakdown (Today)
        $orgBreakdown = Organization::withCount(['entryAttempts as today_entries' => function ($q) use ($today) {
            $q->where('business_date', $today)->where('result', 1);
        }, 'entryAttempts as today_persons' => function ($q) use ($today) {
            $q->where('business_date', $today)->where('result', 1)->selectRaw('COALESCE(SUM(total_persons), 0)');
        }])->get()->map(function ($org) {
            return [
                'id' => $org->id,
                'name_ar' => $org->name_ar,
                'code' => $org->code,
                'today_entries' => $org->today_entries ?? 0,
                'today_persons' => (int)($org->today_persons ?? 0),
            ];
        });

        // 4. Hourly Distribution (Today)
        $hourlyData = [];
        for ($h = 8; $h <= 23; $h++) {
            $hourLabel = sprintf('%02d:00', $h);
            $hourlyData[$hourLabel] = 0;
        }

        foreach ($todayAttempts as $att) {
            if ($att->result === 1 && $att->attempted_at_utc) {
                // assume UTC to park local time approx
                $hour = (int)$att->attempted_at_utc->format('H');
                $key = sprintf('%02d:00', $hour);
                if (isset($hourlyData[$key])) {
                    $hourlyData[$key] += $att->total_persons;
                }
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
                'last_heartbeat' => $lastHeartbeatDiff,
                'last_sync' => $latestSync?->created_at?->diffForHumans() ?? '—',
            ],
            'kpis' => [
                'today_allowed' => $todayAllowed,
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
