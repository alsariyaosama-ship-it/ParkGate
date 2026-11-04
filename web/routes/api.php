<?php

use App\Http\Controllers\Api\SyncApiController;
use App\Http\Middleware\VerifySyncApiKey;
use Illuminate\Support\Facades\Route;

Route::middleware([VerifySyncApiKey::class])->group(function () {
    Route::post('/sync/push', [SyncApiController::class, 'push']);
    Route::post('/sync/heartbeat', [SyncApiController::class, 'heartbeat']);
});
