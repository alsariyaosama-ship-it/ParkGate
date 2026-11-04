<?php

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

class VerifySyncApiKey
{
    /**
     * Handle an incoming request.
     * Validates X-Api-Key header sent by ParkGate.SyncAgent.
     */
    public function handle(Request $request, Closure $next): Response
    {
        $expectedKey = config('parkgate.api_key');
        $providedKey = $request->header('X-Api-Key');

        if (empty($expectedKey) || empty($providedKey) || !hash_equals($expectedKey, $providedKey)) {
            return response()->json([
                'message' => 'Unauthorized: Invalid or missing API key.',
            ], 401);
        }

        return $next($request);
    }
}
