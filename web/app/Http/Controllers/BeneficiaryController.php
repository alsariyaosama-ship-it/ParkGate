<?php

namespace App\Http\Controllers;

use App\Models\Beneficiary;
use App\Models\Organization;
use Illuminate\Http\Request;
use Inertia\Inertia;
use Inertia\Response;

class BeneficiaryController extends Controller
{
    public function index(Request $request): Response
    {
        $search = $request->input('search');
        $orgId = $request->input('organization_id');
        $status = $request->input('status');

        $query = Beneficiary::with('organization');

        if (!empty($search)) {
            $query->where(function ($q) use ($search) {
                $q->where('full_name', 'like', "%{$search}%")
                  ->orWhere('card_number', 'like', "%{$search}%")
                  ->orWhere('phone', 'like', "%{$search}%")
                  ->orWhere('national_id', 'like', "%{$search}%");
            });
        }

        if (!empty($orgId)) {
            $query->where('organization_id', $orgId);
        }

        if (!empty($status)) {
            $query->where('status', (int)$status);
        }

        $beneficiaries = $query->latest('created_at_utc')
            ->paginate(15)
            ->withQueryString()
            ->through(function ($b) {
                return [
                    'id' => $b->id,
                    'full_name' => $b->full_name,
                    'card_number' => $b->card_number,
                    'phone' => $b->phone,
                    'national_id' => $b->national_id,
                    'organization_name' => $b->organization?->name_ar ?? '—',
                    'status' => $b->status,
                    'last_entry' => $b->last_granted_entry_utc ? $b->last_granted_entry_utc->format('Y-m-d H:i') : null,
                ];
            });

        $organizations = Organization::where('is_active', true)->get(['id', 'name_ar']);

        return Inertia::render('Beneficiaries/Index', [
            'beneficiaries' => $beneficiaries,
            'organizations' => $organizations,
            'filters' => [
                'search' => $search,
                'organization_id' => $orgId,
                'status' => $status,
            ],
        ]);
    }
}
