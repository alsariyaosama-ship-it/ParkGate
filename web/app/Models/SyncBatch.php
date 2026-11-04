<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SyncBatch extends Model
{
    public $timestamps = false;

    protected $fillable = [
        'records_count',
        'status',
        'error_message',
        'ip_address',
        'created_at',
    ];

    protected $casts = [
        'records_count' => 'integer',
        'created_at' => 'datetime',
    ];
}
