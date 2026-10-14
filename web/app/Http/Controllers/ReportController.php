<?php

namespace App\Http\Controllers;

use App\Models\EntryAttempt;
use App\Models\Organization;
use Carbon\Carbon;
use Illuminate\Http\Request;
use Inertia\Inertia;
use Inertia\Response;

class ReportController extends Controller
{
    public function index(Request $request): Response
    {
        $startDate = $request->input('start_date', Carbon::today()->toDateString());
        $endDate = $request->input('end_date', Carbon::today()->toDateString());
        $orgId = $request->input('organization_id');
        $result = $request->input('result');

        $query = EntryAttempt::with(['beneficiary', 'organization'])
            ->whereBetween('business_date', [$startDate, $endDate]);

        if (!empty($orgId)) {
            $query->where('organization_id', $orgId);
        }

        if (!empty($result)) {
            $query->where('result', (int)$result);
        }

        $totals = [
            'total_attempts' => (clone $query)->count(),
            'total_allowed' => (clone $query)->where('result', 1)->count(),
            'total_denied' => (clone $query)->where('result', 2)->count(),
            'total_overridden' => (clone $query)->where('result', 3)->count(),
            'total_persons' => (clone $query)->where('result', 1)->sum('total_persons'),
        ];

        $attempts = $query->latest('attempted_at_utc')
            ->paginate(20)
            ->withQueryString()
            ->through(function ($a) {
                return [
                    'id' => $a->id,
                    'beneficiary_name' => $a->beneficiary?->full_name ?? '—',
                    'card_number' => $a->beneficiary?->card_number ?? '—',
                    'organization_name' => $a->organization?->name_ar ?? '—',
                    'business_date' => $a->business_date,
                    'time_formatted' => $a->attempted_at_utc ? $a->attempted_at_utc->format('H:i') : '—',
                    'result' => $a->result,
                    'denial_reason' => $a->denial_reason,
                    'total_persons' => $a->total_persons,
                    'override_reason' => $a->override_reason,
                    'is_voided' => $a->is_voided,
                ];
            });

        $organizations = Organization::where('is_active', true)->get(['id', 'name_ar']);

        return Inertia::render('Reports/Index', [
            'attempts' => $attempts,
            'totals' => $totals,
            'organizations' => $organizations,
            'filters' => [
                'start_date' => $startDate,
                'end_date' => $endDate,
                'organization_id' => $orgId,
                'result' => $result,
            ],
        ]);
    }
}
