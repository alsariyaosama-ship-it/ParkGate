<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\ParkHeartbeat;
use App\Models\SyncBatch;
use App\Services\SyncIngestionService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Log;

class SyncApiController extends Controller
{
    protected SyncIngestionService $syncService;

    public function __construct(SyncIngestionService $syncService)
    {
        $this->syncService = $syncService;
    }

    /**
     * POST /api/sync/push
     * Accepts a batch of records from ParkGate.SyncAgent.
     */
    public function push(Request $request): JsonResponse
    {
        $records = $request->input('records');

        if (!is_array($records)) {
            return response()->json([
                'success' => false,
                'message' => 'Malformed payload: "records" array is required.',
            ], 422);
        }

        if (count($records) === 0) {
            return response()->json([
                'success' => true,
                'message' => 'Empty batch received.',
                'processed' => 0,
            ]);
        }

        try {
            $stats = $this->syncService->ingestBatch($records);

            SyncBatch::create([
                'records_count' => count($records),
                'status' => 'Success',
                'ip_address' => $request->ip(),
                'created_at' => now(),
            ]);

            return response()->json([
                'success' => true,
                'message' => 'Batch ingested successfully.',
                'processed' => $stats['processed'],
            ], 200);
        } catch (\Throwable $e) {
            SyncBatch::create([
                'records_count' => count($records),
                'status' => 'Failed',
                'error_message' => $e->getMessage(),
                'ip_address' => $request->ip(),
                'created_at' => now(),
            ]);

            Log::error("Sync push error: {$e->getMessage()}");

            return response()->json([
                'success' => false,
                'message' => 'Failed to process sync batch: ' . $e->getMessage(),
            ], 500);
        }
    }

    /**
     * POST /api/sync/heartbeat
     * Records a heartbeat ping from the park sync agent.
     */
    public function heartbeat(Request $request): JsonResponse
    {
        ParkHeartbeat::create([
            'last_heartbeat_at' => now(),
            'ip_address' => $request->ip(),
            'created_at' => now(),
        ]);

        return response()->json([
            'success' => true,
            'status' => 'online',
            'timestamp' => now()->toIso8601String(),
        ]);
    }
}
