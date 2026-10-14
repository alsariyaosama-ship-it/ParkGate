<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ParkHeartbeat extends Model
{
    public $timestamps = false;

    protected $fillable = [
        'last_heartbeat_at',
        'ip_address',
        'created_at',
    ];

    protected $casts = [
        'last_heartbeat_at' => 'datetime',
        'created_at' => 'datetime',
    ];
}
