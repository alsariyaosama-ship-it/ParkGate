<?php

namespace App\Http\Controllers\Api;

use App\Exceptions\SyncItemException;
use App\Http\Controllers\Controller;
use App\Http\Requests\SyncPushRequest;
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
    public function push(SyncPushRequest $request): JsonResponse
    {
        $records = $request->validated('records');

        if (count($records) === 0) {
            return response()->json([
                'success' => true,
                'message' => 'Empty batch received.',
                'processed' => 0,
            ]);
        }

        try {
            $stats = $this->syncService->ingestBatch($records);
        } catch (\Throwable $e) {
            return $this->failure($e, $request, count($records));
        }

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
    }

    /**
     * POST /api/sync/heartbeat
     * Records a heartbeat ping from the park sync agent.
     */
    public function heartbeat(Request $request): JsonResponse
    {
        // R2.5: how many outbox rows wait to be sent, and how many gave up (retry limit reached).
        $counts = $request->validate([
            'pendingCount' => ['nullable', 'integer', 'min:0', 'max:4294967295'],
            'failedCount' => ['nullable', 'integer', 'min:0', 'max:4294967295'],
        ]);

        ParkHeartbeat::create([
            'last_heartbeat_at' => now(),
            'pending_count' => $counts['pendingCount'] ?? 0,
            'failed_count' => $counts['failedCount'] ?? 0,
            'ip_address' => $request->ip(),
            'created_at' => now(),
        ]);

        return response()->json([
            'success' => true,
            'status' => 'online',
            'timestamp' => now()->toIso8601String(),
        ]);
    }

    /**
     * R1.6: the caller gets a generic message and an error id. The log gets the
     * error id, entity type/id and exception class only: exception messages can
     * carry payload values (for example SQL bindings), so they are never logged.
     */
    private function failure(\Throwable $e, Request $request, int $recordCount): JsonResponse
    {
        $errorId = bin2hex(random_bytes(6));
        $rejected = $e instanceof SyncItemException && $e->isRejectedPayload();
        $cause = $e instanceof SyncItemException ? $e->getPrevious() : $e;

        $context = [
            'error_id' => $errorId,
            'entity_type' => $e instanceof SyncItemException ? $e->entityType : null,
            'entity_id' => $e instanceof SyncItemException ? $e->entityId : null,
            'exception' => $cause ? $cause::class : $e::class,
            'records' => $recordCount,
        ];

        SyncBatch::create([
            'records_count' => $recordCount,
            'status' => 'Failed',
            'error_message' => "error_id={$errorId}",
            'ip_address' => $request->ip(),
            'created_at' => now(),
        ]);

        if ($rejected) {
            Log::warning('Sync push rejected an item.', $context);

            return response()->json([
                'success' => false,
                'message' => 'Unprocessable sync item.',
                'error_id' => $errorId,
            ], 422);
        }

        Log::error('Sync push failed.', $context);

        return response()->json([
            'success' => false,
            'message' => 'Failed to process sync batch.',
            'error_id' => $errorId,
        ], 500);
    }
}
