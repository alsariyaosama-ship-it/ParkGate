<?php

use App\Http\Controllers\Api\SyncApiController;
use App\Http\Middleware\VerifySyncApiKey;
use Illuminate\Support\Facades\Route;

// R1.6: throttle first, so failed key guesses count too. The agent pushes and
// sends a heartbeat every 30 s by default, far below 60 requests a minute.
Route::middleware(['throttle:60,1', VerifySyncApiKey::class])->group(function () {
    Route::post('/sync/push', [SyncApiController::class, 'push']);
    Route::post('/sync/heartbeat', [SyncApiController::class, 'heartbeat']);
});
