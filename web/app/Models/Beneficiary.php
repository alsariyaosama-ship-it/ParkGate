<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\Relations\HasMany;

class Beneficiary extends Model
{
    public $timestamps = false;
    public $incrementing = false;
    protected $keyType = 'string';

    protected $fillable = [
        'id',
        'organization_id',
        'card_number',
        'full_name',
        'phone',
        'national_id',
        'card_expiry_date',
        'status',
        'status_reason',
        'notes',
        'last_granted_entry_utc',
        'created_at_utc',
        'updated_at_utc',
    ];

    protected $casts = [
        'card_expiry_date' => 'date',
        'status' => 'integer',
        'last_granted_entry_utc' => 'datetime',
        'created_at_utc' => 'datetime',
        'updated_at_utc' => 'datetime',
    ];

    public function organization(): BelongsTo
    {
        return $this->belongsTo(Organization::class);
    }

    public function entryAttempts(): HasMany
    {
        return $this->hasMany(EntryAttempt::class);
    }
}
